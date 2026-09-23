using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DshTray
{
    static partial class Program
    {
        static string _runtimePackage;
        static bool _pluginMode;
        static volatile string _launchUrl;
        static volatile bool _authenticated;
        static volatile bool _readyAnnounced;
        static readonly List<string> _attemptLines = new List<string>();
        static string RecoveryPath => Path.Combine(_dataDir, "recovery.json");
        static string ReportPath => Path.Combine(_dataDir, "plugin-diagnostics.txt");
        static string CoreProfileName => "dsh-tray-core-" + GetLocalVersion();

        static string Redact(string text) => Regex.Replace(text ?? "", @"(?i)(token=)[^\s&]+", "$1[REDACTED]");

        static void LoadRecoverySettings()
        {
            // Older settings without a saved mode retain their core-mode default.
            if (!File.Exists(RecoveryPath)) return;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(RecoveryPath));
                _pluginMode = doc.RootElement.TryGetProperty("pluginMode", out var mode) && mode.ValueKind == JsonValueKind.True;
                string path = doc.RootElement.GetProperty("runtimePackage").GetString();
                if (!string.IsNullOrEmpty(path))
                {
                    if (!AllowedRuntime(path))
                        throw new InvalidOperationException("已保存的运行时不存在或不在托盘运行时目录");
                    _runtimePackage = path;
                }
            }
            catch (Exception ex) { Log("runtime selection: " + ex.Message); }
        }

        static void SaveRuntime()
        {
            string temp = RecoveryPath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(new { runtimePackage = _runtimePackage, pluginMode = _pluginMode }), Encoding.UTF8);
            File.Move(temp, RecoveryPath, true);
        }

        static bool UsesRemoteApi()
        {
            // npm --prefix hoists packages; global npm nests them below dsh.
            for (var dir = new DirectoryInfo(Path.GetDirectoryName(LocalPackagePath)); dir != null; dir = dir.Parent)
                if (File.Exists(Path.Combine(dir.FullName, "node_modules", "@deepseek-ai", "dsh-api-gateway", "package.json"))) return true;
            return false;
        }

        static bool EmptyPatch(string path)
        {
            if (!File.Exists(path)) return true;
            string text = Regex.Replace(File.ReadAllText(path), @"(?m)#.*$", "").Trim();
            return text == "" || text == "[]";
        }

        static void PrepareCoreProfile()
        {
            // Separate composition, shared Harness home: sessions and credentials
            // remain at their original paths. Never rewrite the user's web profile.
            if (!EmptyPatch(Path.Combine(DshHome, "cordis.patch.yml")))
                throw new InvalidOperationException("全局 cordis.patch.yml 非空，无法保证核心隔离；已拒绝加载，请先检查全局补丁。");
            string dir = Path.Combine(DshHome, "profiles", CoreProfileName);
            Directory.CreateDirectory(dir);
            string manifest = Path.Combine(dir, "package.json");
            if (!File.Exists(manifest))
            {
                File.WriteAllText(manifest, JsonSerializer.Serialize(new {
                    name = CoreProfileName,
                    @private = true,
                    dependencies = new Dictionary<string, string>(),
                    dsh = new { profile = new { bundles = new[] { "@deepseek-ai/dsh-base", "@deepseek-ai/dsh-web-app" }, patchReload = "startup" } }
                }), new UTF8Encoding(false));
            }
            using var doc = JsonDocument.Parse(File.ReadAllText(manifest));
            var root = doc.RootElement;
            var bundles = root.GetProperty("dsh").GetProperty("profile").GetProperty("bundles").EnumerateArray().Select(x => x.GetString()).ToArray();
            if (!bundles.SequenceEqual(new[] { "@deepseek-ai/dsh-base", "@deepseek-ai/dsh-web-app" })
                || root.GetProperty("dependencies").EnumerateObject().Any()
                || !EmptyPatch(Path.Combine(dir, "cordis.patch.yml"))
                || (Directory.Exists(Path.Combine(dir, "node_modules")) && Directory.EnumerateFileSystemEntries(Path.Combine(dir, "node_modules")).Any()))
                throw new InvalidOperationException("核心 profile 被修改或装入依赖，已拒绝加载: " + dir);
        }

        static bool WaitForHealthy()
        {
            for (int i = 0; i < 45; i++)
            {
                if (!IsManagedServiceAlive()) return false;
                if (IsServiceRunning()) return true;
                Thread.Sleep(1000);
            }
            return false;
        }

        static bool EnsureHealthyService()
        {
            StartService();
            if (WaitForHealthy()) return true;
            if (!StopService()) return false;
            if (!_pluginMode) return false;
            CapturePluginFailure();
            WritePluginReport();
            _pluginMode = false;
            SaveRuntime();
            Log("plugin trial failed; retaining runtime, falling back to core");
            StartService();
            bool recovered = WaitForHealthy();
            if (!recovered) StopService();
            return recovered;
        }

        static void SwitchMode(bool plugins)
        {
            _menu.Enabled = false;
            try
            {
                if (!StopService()) throw new InvalidOperationException("现有服务无法安全停止");
                // Persisted runtime is kept even when plugin activation fails.
                _pluginMode = plugins;
                bool ready = EnsureHealthyService();
                SaveRuntime();
                Msg(ready ? (plugins && !_pluginMode ? "插件模式失败，已恢复核心模式。查看“插件诊断与修复命令”。" : "DSH 已就绪。") : "核心启动失败，请查看日志。",
                    "DeepSeek Harness", MessageBoxButtons.OK, ready ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex) { Msg(ex.Message, "DeepSeek Harness", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { _menu.Enabled = true; UpdateStatusAsync(); }
        }

        static void WritePluginReport()
        {
            var text = new StringBuilder("插件模式未通过启动/API 验收。已尝试恢复核心，本体版本不因插件失败回退。\r\n");
            text.AppendLine("以下命令仅供手动修复，不会自动执行；安装新版不代表兼容性已验证。修复后从托盘再次尝试插件模式。");
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(DshHome, "profiles", "web", "package.json")));
                var bundles = doc.RootElement.GetProperty("dsh").GetProperty("profile").GetProperty("bundles");
                bool hasDependencies = doc.RootElement.TryGetProperty("dependencies", out var dependencies);
                foreach (var item in bundles.EnumerateArray())
                {
                    string package = item.GetString();
                    if (package.StartsWith("@deepseek-ai/", StringComparison.Ordinal)) continue;
                    if (!Regex.IsMatch(package, @"^(?:@[a-z0-9][a-z0-9._-]*/)?[a-z0-9][a-z0-9._-]*$")) continue;
                    text.AppendLine("待核查: " + package);
                    if (hasDependencies && dependencies.TryGetProperty(package, out var source)
                        && Regex.IsMatch(source.GetString() ?? "", @"^[~^<>=*0-9xX .|+a-z-]+$")
                        && !source.GetString().Contains(":"))
                        text.AppendLine("  请在 Beta 的官方插件管理器中检查此包更新，不使用全局 dsh 命令。");
                    else text.AppendLine("  本地/仓库/自定义来源：请按原来源维护，未生成可能无效的 npm 更新命令。");
                }
            }
            catch { text.AppendLine("未能读取插件清单。"); }
            text.AppendLine("本次启动日志（仅代表加载/接口验收，不能证明所有插件业务功能）：");
            lock (_serviceLogLock) foreach (string line in _attemptLines) text.AppendLine(line);
            File.WriteAllText(ReportPath, text.ToString(), Encoding.UTF8);
        }

        static void ShowPluginReport()
        {
            if (!File.Exists(ReportPath))
                File.WriteAllText(ReportPath, "尚无插件失败记录。核心模式不会加载第三方插件；可从托盘尝试插件模式。", Encoding.UTF8);
            var psi = new ProcessStartInfo("notepad.exe") { UseShellExecute = true };
            psi.ArgumentList.Add(ReportPath);
            Process.Start(psi);
        }
    }
}
