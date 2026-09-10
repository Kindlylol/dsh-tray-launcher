using System;
using System.IO;
using System.Text.Json;

namespace DshTray
{
    static partial class Program
    {
        static string _testHome;
        static void RunRecoveryTests(string[] args)
        {
            if (args.Length != 2 && args.Length != 3) { Environment.ExitCode = 2; return; }
            _silent = true;
            _testHome = Path.GetFullPath(args[1]);
            _dataDir = Path.Combine(_testHome, "tray-test");
            Directory.CreateDirectory(_dataDir);
            _logPath = Path.Combine(_dataDir, "tray.log");
            bool ready = false;
            int checks = 0;
            try
            {
                if (args[0] == "--contract-tests")
                {
                    void Check(bool value) { checks++; if (!value) throw new Exception("Contract failed: " + checks); }
                    Check(IsHealthyDescribeResponse("{\"rpcId\":\"a\",\"result\":{\"ok\":true}}", "a"));
                    Check(!IsHealthyDescribeResponse("{\"rpcId\":\"b\",\"result\":{\"ok\":true}}", "a"));
                    Check(!IsHealthyDescribeResponse("{\"result\":{\"ok\":true}}", "a"));
                    Check(!IsHealthyDescribeResponse("{\"rpcId\":\"a\",\"result\":{\"ok\":false}}", "a"));
                    Check(!IsHealthyDescribeResponse("<html>OK</html>", "a"));
                    Check(Redact("http://localhost/?token=secret") == "http://localhost/?token=[REDACTED]");
                    PrepareCoreProfile(); Check(true);
                    string patch = Path.Combine(DshHome, "profiles", CoreProfileName, "cordis.patch.yml");
                    File.WriteAllText(patch, "# empty\n[]\n"); PrepareCoreProfile(); Check(true);
                    File.WriteAllText(patch, "- id: bad\n  name: third-party\n");
                    bool blocked = false;
                    try { PrepareCoreProfile(); } catch (InvalidOperationException) { blocked = true; }
                    Check(blocked);
                    File.WriteAllText(patch, "[]");
                    File.WriteAllText(Path.Combine(DshHome, "cordis.patch.yml"), "- id: global\n");
                    blocked = false;
                    try { PrepareCoreProfile(); } catch (InvalidOperationException) { blocked = true; }
                    Check(blocked);
                    ready = true;
                }
                else
                {
                    _pluginMode = args[0] == "--test-fallback";
                    ready = EnsureHealthyService();
                    if (args[0] == "--test-install" && ready)
                    {
                        InstallCoreUpdate("invalid;target");
                        if (!IsServiceRunning() || _runtimePackage != null) throw new Exception("Invalid target changed active runtime");
                        InstallCoreUpdate(args[2]);
                        ready = IsServiceRunning() && _runtimePackage != null && GetLocalVersion() == args[2];
                        if (ready)
                        {
                            string installed = _runtimePackage;
                            InstallCoreUpdate("999.999.999");
                            ready = IsServiceRunning() && _runtimePackage == installed;
                            checks = 3;
                        }
                    }
                    if (_pluginMode) ready = false;
                    if (args[0] == "--test-fallback" && !File.Exists(ReportPath)) ready = false;
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }
            finally
            {
                if (args[0] != "--contract-tests" && !StopService()) ready = false;
                File.WriteAllText(Path.Combine(_dataDir, "result.json"), JsonSerializer.Serialize(new { ready, checks, core = !_pluginMode, version = GetLocalVersion() }));
                Environment.ExitCode = ready ? 0 : 1;
            }
        }
    }
}
