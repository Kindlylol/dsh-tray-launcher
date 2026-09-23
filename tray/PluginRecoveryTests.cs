using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace DshTray
{
    static partial class Program
    {
        static void CreateRepairFixture(string home)
        {
            string dir = Path.Combine(home, "profiles", "web");
            if (Directory.Exists(dir)) throw new IOException("测试目录已存在，拒绝覆盖。");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "package.json"), "{\"private\":true,\"dependencies\":{\"broken-demo\":\"1.0.0\",\"working-demo\":\"1.0.0\"},\"dsh\":{\"profile\":{\"bundles\":[\"@deepseek-ai/dsh-base\",\"@deepseek-ai/dsh-web-app\",\"broken-demo\",\"working-demo\"]}}}");
            File.WriteAllText(Path.Combine(dir, "cordis.patch.yml"), "[]\n");
            File.WriteAllText(Path.Combine(dir, "pnpm-lock.yaml"), "lockfileVersion: '9.0'\n");
        }
        static void RunRepairUiFixture(string home)
        {
            CreateRepairFixture(home);
            Application.Run(new PluginRecoveryForm(home, () => true,
                () => System.Threading.Tasks.Task.FromResult("界面测试：仅验证清单变化，未启动任何 DSH 后台。"),
                () => "Plugin broken-demo@1.0.0 is incompatible with dsh 0.1.6-alpha.2: peerDependencies mismatch"));
        }
        static void RunRepairIntegration(string home, string runtime)
        {
            _silent = true; _testHome = Path.GetFullPath(home); _runtimePackage = Path.GetFullPath(runtime);
            _dataDir = Path.Combine(_testHome, "tray-state"); Directory.CreateDirectory(_dataDir);
            _logPath = Path.Combine(_dataDir, "tray.log");
            int checks = 0;
            void Check(bool ok) { checks++; if (!ok) throw new Exception("Integration failed: " + checks); }
            try
            {
                if (IsPortOpen()) throw new IOException("Beta 测试端口已占用，未启动。");
                CreateRepairFixture(home);
                foreach (string name in new[] { "broken-demo", "working-demo" })
                {
                    string dir = Path.Combine(home, "profiles", "web", "node_modules", name);
                    Directory.CreateDirectory(dir);
                    File.WriteAllText(Path.Combine(dir, "package.json"), JsonSerializer.Serialize(new {
                        name, version = "1.0.0", type = "module", main = "index.js", dsh = new { bundle = new { patch = "./cordis.patch.yml" } }
                    }));
                    File.WriteAllText(Path.Combine(dir, "cordis.patch.yml"), "- insert:\n    - id: " + name + "\n      name: " + name + "\n");
                    File.WriteAllText(Path.Combine(dir, "index.js"), name == "broken-demo"
                        ? "throw new Error('BETA_FIXTURE_FAILURE: broken-demo'); export default function() {}"
                        : "import fs from 'node:fs'; import path from 'node:path'; export default function() { fs.writeFileSync(path.join(process.env.DSH_HOME,'working.marker'),'active'); }");
                }
                _pluginMode = true;
                Check(EnsureHealthyService() && !_pluginMode);
                Check(File.Exists(PluginFailurePath) && File.ReadAllText(PluginFailurePath).Contains("BETA_FIXTURE_FAILURE"));
                Check(StopForRepair());
                var store = new PluginRecoveryStore(home);
                store.Apply(store.Read(), "broken-demo", false);
                _pluginMode = true;
                Check(EnsureHealthyService() && _pluginMode);
                Check(File.Exists(Path.Combine(home, "working.marker")) && PluginModeLabel() == "部分插件模式");
                Check(StopForRepair());
                store.Apply(store.Read(), "broken-demo", true);
                _pluginMode = true;
                Check(EnsureHealthyService() && !_pluginMode);
                Check(store.Read().Rows.All(r => r.Enabled)); // Retry failure never silently rewrites the user's choice.
                File.WriteAllText(Path.Combine(home, "integration-result.json"), JsonSerializer.Serialize(new { passed = true, checks, runtime = GetLocalVersion() }));
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(home, "integration-result.json"), JsonSerializer.Serialize(new { passed = false, checks, error = ex.ToString() }));
                Environment.ExitCode = 1;
            }
            finally { StopService(); }
        }
        static void RunPluginRecoveryTests(string home)
        {
            int checks = 0;
            void Check(bool ok) { checks++; if (!ok) throw new Exception("Recovery test failed: " + checks); }
            void Reject(Action action) { bool rejected = false; try { action(); } catch { rejected = true; } Check(rejected); }
            try
            {
                CreateRepairFixture(home);
                var store = new PluginRecoveryStore(home);
                var initial = store.Read();
                Check(initial.Rows.Count == 2 && initial.Rows.All(r => r.Enabled));
                Check(initial.Rows.All(r => r.Evidence.Contains("原因未确定")));
                Check(PluginRecoveryStore.Evidence("broken-demo", "at broken-demo/index.js:4").Contains("原因未确定"));
                Check(PluginRecoveryStore.Evidence("broken-demo", "Plugin broken-demo@1.0.0 is incompatible with dsh 0.1.6").Contains("候选"));
                Check(PluginRecoveryStore.Evidence("broken", "Plugin broken-demo@1.0.0 is incompatible with dsh 0.1.6").Contains("原因未确定"));
                string backup = store.Apply(initial, "broken-demo", false);
                Check(File.ReadAllText(backup) == initial.Text);
                var partial = store.Read();
                Check(!partial.Rows.Single(r => r.Name == "broken-demo").Enabled && partial.Rows.Single(r => r.Name == "working-demo").Enabled);
                Check(JsonDocument.Parse(partial.Text).RootElement.GetProperty("dependencies").GetProperty("broken-demo").GetString() == "1.0.0");
                Reject(() => store.Apply(initial, "working-demo", false));
                Reject(() => store.Apply(partial, "@deepseek-ai/dsh-base", false));
                string patch = Path.Combine(home, "profiles", "web", "cordis.patch.yml");
                File.AppendAllText(patch, "# concurrent\n");
                Reject(() => store.Apply(partial, "broken-demo", true));
                var refreshed = store.Read();
                using (var gate = new FileStream(Path.Combine(home, "profiles", "web", ".tray-recovery.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    Reject(() => store.Apply(refreshed, "broken-demo", true));
                store.Apply(refreshed, "broken-demo", true);
                var restored = store.Read();
                Check(restored.Rows.All(r => r.Enabled));
                var order = restored.Rows.Select(r => r.Name).ToArray();
                store.Apply(restored, "broken-demo", false);
                store.Apply(store.Read(), "working-demo", false);
                store.Apply(store.Read(), "broken-demo", true);
                store.Apply(store.Read(), "working-demo", true);
                Check(store.Read().Rows.Select(r => r.Name).SequenceEqual(order));
                Check(File.ReadAllText(patch) == "[]\n# concurrent\n");
                Check(File.ReadAllText(Path.Combine(home, "profiles", "web", "pnpm-lock.yaml")) == "lockfileVersion: '9.0'\n");
                string secret = RedactDiagnostic("token=secret bearer abc api_key=xyz https://user:pass@example.com");
                Check(!secret.Contains("secret") && !secret.Contains("abc") && !secret.Contains("xyz") && !secret.Contains("user:pass"));
                var malformed = System.Text.Json.Nodes.JsonNode.Parse(initial.Text).AsObject();
                malformed["dshTrayRecovery"] = new System.Text.Json.Nodes.JsonArray();
                File.WriteAllText(store.Manifest, malformed.ToJsonString());
                Reject(() => store.Read());
                File.WriteAllText(store.Manifest, "bad-json");
                Reject(() => store.Read());
                Check(File.ReadAllText(backup) == initial.Text);
                File.WriteAllText(Path.Combine(home, "result.json"), JsonSerializer.Serialize(new { passed = true, checks }));
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(home);
                File.WriteAllText(Path.Combine(home, "result.json"), JsonSerializer.Serialize(new { passed = false, checks, error = ex.ToString() }));
                Environment.ExitCode = 1;
            }
        }
    }
}
