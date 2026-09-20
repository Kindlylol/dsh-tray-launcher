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
                    _pluginMode = true; SaveRuntime(); _pluginMode = false; LoadRecoverySettings();
                    Check(_pluginMode);
                    _pluginMode = false; SaveRuntime(); _pluginMode = true; LoadRecoverySettings();
                    Check(!_pluginMode);
                    File.WriteAllText(RecoveryPath, "{\"runtimePackage\":null}"); LoadRecoverySettings();
                    Check(!_pluginMode);
                    const string registry = "{\"dist-tags\":{\"latest\":\"0.1.5-rc.1\",\"alpha\":\"0.1.6-alpha.1\"},\"versions\":{\"0.1.5-rc.1\":{\"version\":\"0.1.5-rc.1\"},\"0.1.6-alpha.1\":{\"version\":\"0.1.6-alpha.1\"}}}";
                    var channels = ParseUpdateChannels(registry);
                    Check(channels.Count == 2 && channels[0].Source == "latest" && channels[1].Source == "alpha");
                    Check(channels[0].Installable && channels[1].Installable);
                    Check(!CanUpdate("0.1.5-rc.1", channels[0]) && CanUpdate("0.1.5-rc.1", channels[1]));
                    Check(CanUpdate("0.1.4", channels[0]) && CanUpdate("0.1.4", channels[1]));
                    Check(!CanUpdate("0.1.6-alpha.1", channels[0]) && !CanUpdate("0.1.6-alpha.1", channels[1]));
                    Check(!CanUpdate("未知", channels[1]));
                    Check(CompareVersions("0.1.10-alpha.1", "0.1.6-alpha.1") > 0);
                    Check(CompareVersions("0.1.6-alpha.10", "0.1.6-alpha.2") > 0);
                    Check(CompareVersions("0.1.6", "0.1.6-alpha.1") > 0);
                    var missing = ParseUpdateChannels("{\"dist-tags\":{\"latest\":\"1.0.0\"},\"versions\":{}}");
                    Check(!missing[0].Installable && !missing[1].Installable && missing[1].Version == null);
                    var invalid = ParseUpdateChannels(registry.Replace("0.1.6-alpha.1", "bad;command"));
                    Check(invalid[0].Installable && !invalid[1].Installable);
                    bool rejected = false;
                    try { ParseUpdateChannels("{}"); } catch (InvalidDataException) { rejected = true; }
                    Check(rejected);
                    using (var form = new UpdateChannelForm("0.1.5-rc.1", channels))
                    {
                        Check(!form.Choices[0].Enabled && form.Choices[1].Enabled);
                        Check(form.SelectedVersion == null && !form.InstallButton.Enabled);
                        form.Choices[1].Checked = true;
                        Check(form.SelectedVersion == channels[1] && form.InstallButton.Enabled);
                    }
                    using (var form = new UpdateChannelForm("0.1.4", channels))
                    {
                        form.Choices[0].Checked = true;
                        Check(form.SelectedVersion == channels[0]);
                        form.Choices[1].Checked = true;
                        Check(form.SelectedVersion == channels[1] && !form.Choices[0].Checked);
                    }
                    Check(IsHealthyDescribeResponse("{\"rpcId\":\"a\",\"result\":{\"ok\":true}}", "a"));
                    Check(!IsHealthyDescribeResponse("{\"rpcId\":\"b\",\"result\":{\"ok\":true}}", "a"));
                    Check(!IsHealthyDescribeResponse("{\"result\":{\"ok\":true}}", "a"));
                    Check(!IsHealthyDescribeResponse("{\"rpcId\":\"a\",\"result\":{\"ok\":false}}", "a"));
                    Check(!IsHealthyDescribeResponse("<html>OK</html>", "a"));
                    Check(Redact("http://localhost/?token=secret") == "http://localhost/?token=[REDACTED]");
                    string fixture = Path.Combine(_dataDir, "runtimes", "fixture", "package.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(fixture));
                    File.WriteAllText(fixture, "{\"version\":\"0.1.6-alpha.2\"}");
                    _runtimePackage = fixture;
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
