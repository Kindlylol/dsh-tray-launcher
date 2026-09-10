using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DshTray
{
    static partial class Program
    {
        static void InstallCoreUpdate(string targetVersion)
        {
            string previous = _runtimePackage;
            bool wasAlive = IsManagedServiceAlive();
            bool switched = false;
            if (_menu != null) _menu.Enabled = false;
            try
            {
                if (!Regex.IsMatch(targetVersion ?? "", @"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$"))
                    throw new InvalidOperationException("目标版本格式无效");
                if (IsPortOpen() && !wasAlive)
                    throw new InvalidOperationException("端口被非受管进程占用，无法验收更新");
                // Install into a new directory while the existing service is still
                // available. Never let npm mutate the only usable installation.
                string stage = Path.Combine(_dataDir, "runtimes", targetVersion + "-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(stage);
                string npmCli = Path.Combine(Path.GetDirectoryName(FindNode()), "node_modules", "npm", "bin", "npm-cli.js");
                if (!File.Exists(npmCli)) throw new FileNotFoundException("未找到 Node 随附的 npm CLI", npmCli);
                var psi = new ProcessStartInfo(FindNode())
                {
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true,
                    WorkingDirectory = stage
                };
                foreach (string arg in new[] { npmCli, "install", "--prefix", stage, "--no-audit", "--no-fund", "--registry=https://registry.npmjs.org", "@deepseek-ai/dsh@" + targetVersion })
                    psi.ArgumentList.Add(arg);
                using (var proc = Process.Start(psi))
                {
                    if (_silent)
                    {
                        Task<string> stdout = proc.StandardOutput.ReadToEndAsync();
                        Task<string> stderr = proc.StandardError.ReadToEndAsync();
                        if (!proc.WaitForExit(300000)) { proc.Kill(true); proc.WaitForExit(); throw new TimeoutException("npm 安装超过 5 分钟"); }
                        Log("npm exit=" + proc.ExitCode);
                        if (proc.ExitCode != 0) Log(stderr.GetAwaiter().GetResult());
                        stdout.GetAwaiter().GetResult();
                    }
                    else
                    {
                        using var form = new ProgressForm(proc, TimeSpan.FromMinutes(5));
                        form.ShowDialog();
                        if (form.TimedOut || form.Canceled) throw new OperationCanceledException("安装已取消或超时，原运行时保持不变");
                    }
                    if (!proc.HasExited || proc.ExitCode != 0) throw new InvalidOperationException("npm 安装未成功");
                }
                string candidate = Path.Combine(stage, "node_modules", "@deepseek-ai", "dsh", "package.json");
                using (var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(candidate)))
                    if (doc.RootElement.GetProperty("version").GetString() != targetVersion)
                        throw new InvalidOperationException("候选运行时版本不匹配");
                if (!StopService()) throw new InvalidOperationException("无法安全停止旧服务");
                _runtimePackage = candidate;
                _pluginMode = false;
                switched = true;
                if (!EnsureHealthyService()) throw new InvalidOperationException("候选核心未通过认证/RPC 验收");
                if (File.Exists(RecoveryPath)) File.Copy(RecoveryPath, RecoveryPath + ".previous", true);
                SaveRuntime();
                Log("core update verified: " + targetVersion);
                Msg("本体更新完成并已通过核心验收: " + targetVersion + "\n当前运行核心模式。可从托盘单独尝试插件模式；插件失败会回到核心。\n旧运行时保留在本机。",
                    "DeepSeek Harness", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("core update failed: " + ex.Message);
                bool recovered = !switched;
                if (switched && StopService())
                {
                    _runtimePackage = previous;
                    _pluginMode = false;
                    SaveRuntime();
                    recovered = !wasAlive || EnsureHealthyService();
                }
                Msg("更新未完成: " + ex.Message + "\n" + (recovered ? "原运行时已保留；若原服务运行中，已恢复核心服务。" : "自动恢复未通过，请查看日志。旧安装仍保留。"),
                    "DeepSeek Harness", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (_menu != null) _menu.Enabled = true; UpdateStatusAsync(); }
        }

        static string FindNode()
        {
            foreach (string dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                string path = Path.Combine(dir.Trim('"'), "node.exe");
                if (File.Exists(path)) return path;
            }
            throw new FileNotFoundException("未找到 node.exe");
        }
    }
}
