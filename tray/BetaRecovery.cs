using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DshTray
{
    static partial class Program
    {
        static PluginRecoveryForm _repairWindow;
        static string BetaRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSH Tray Launcher Beta");
        internal static string RedactDiagnostic(string text)
        {
            text = Redact(text);
            text = Regex.Replace(text, @"(?i)(authorization\s*[:=]\s*|bearer\s+|(?:api[_-]?key|access[_-]?token|password|secret)\s*[:=]\s*)[^\s,;]+", "$1[REDACTED]");
            return Regex.Replace(text, @"(https?://)[^\s/@]+:[^\s/@]+@", "$1[REDACTED]@");
        }
        static void InitializeBeta()
        {
            _testHome ??= Path.Combine(BetaRoot, "home");
            Directory.CreateDirectory(DshHome);
            string profile = Path.Combine(DshHome, "profiles", "web");
            Directory.CreateDirectory(profile);
            string manifest = Path.Combine(profile, "package.json");
            if (!File.Exists(manifest)) File.WriteAllText(manifest,
                "{\"name\":\"dsh-tray-beta-web\",\"private\":true,\"dependencies\":{},\"dsh\":{\"profile\":{\"bundles\":[\"@deepseek-ai/dsh-base\",\"@deepseek-ai/dsh-web-app\"]}}}");
            // Read-only reference to the selected stable runtime. No profile, credentials or sessions are imported.
            if (_runtimePackage == null)
            {
                string stable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSH Tray Launcher", "recovery.json");
                if (File.Exists(stable))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(stable));
                    string path = doc.RootElement.GetProperty("runtimePackage").GetString();
                    if (AllowedRuntime(path)) _runtimePackage = path;
                }
            }
        }
        static bool AllowedRuntime(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
            string full = Path.GetFullPath(path);
            return new[] { _dataDir, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSH Tray Launcher") }
                .Any(root => full.StartsWith(Path.GetFullPath(Path.Combine(root, "runtimes")) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
        }
        static string PluginFailurePath => Path.Combine(_dataDir, "last-plugin-failure.txt");
        static string PluginDiagnostic() => File.Exists(PluginFailurePath) ? File.ReadAllText(PluginFailurePath) : "";
        static void CapturePluginFailure()
        {
            lock (_serviceLogLock) File.WriteAllText(PluginFailurePath, RedactDiagnostic(string.Join(Environment.NewLine, _attemptLines)));
        }
        static string PluginModeLabel()
        {
            try { return new PluginRecoveryStore(DshHome).Read().Rows.Any(r => !r.Enabled) ? "部分插件模式" : "插件模式"; }
            catch { return "插件模式（清单待检查）"; }
        }
        static bool StopForRepair()
        {
            if (!StopService()) return false;
            return !IsPortOpen(); // Never edit while an unowned backend occupies the Beta port.
        }
        static async Task<string> RetryRepairedProfile()
        {
            _pluginMode = true;
            bool ready = await Task.Run(EnsureHealthyService);
            SaveRuntime(); UpdateStatusAsync();
            if (ready) OpenBrowser();
            return ready ? (_pluginMode ? PluginModeLabel() + "已启动，RPC 可用；不代表全部插件功能通过。" : "插件启动失败，已恢复核心；可继续在此修复。") : "启动未通过验收，已停止重试。";
        }
        static void OpenPluginRecovery()
        {
            if (_repairWindow != null && !_repairWindow.IsDisposed) { _repairWindow.Activate(); return; }
            _repairWindow = new PluginRecoveryForm(DshHome, StopForRepair, RetryRepairedProfile, PluginDiagnostic);
            if (_menu != null) _menu.Enabled = false;
            _repairWindow.FormClosed += (_, _) => { if (_menu != null) _menu.Enabled = true; UpdateStatusAsync(); };
            _repairWindow.Show();
        }
    }
}
