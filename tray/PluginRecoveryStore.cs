using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace DshTray
{
    // Only bundle selection is edited. Packages, patches and lockfiles are never repaired implicitly.
    internal sealed class PluginRecoveryStore
    {
        internal sealed record Row(string Name, bool Enabled, string Evidence);
        internal sealed record Snapshot(string Text, string Guard, IReadOnlyList<Row> Rows);
        readonly string directory;
        internal string Manifest => Path.Combine(directory, "package.json");
        internal PluginRecoveryStore(string home) { directory = Path.Combine(Path.GetFullPath(home), "profiles", "web"); }
        static bool Core(string name) => name.StartsWith("@deepseek-ai/", StringComparison.Ordinal);
        static JsonArray Bundles(JsonObject root) => root["dsh"]?["profile"]?["bundles"] as JsonArray
            ?? throw new InvalidDataException("插件 profile 缺少 bundles 清单，未做修改。");
        static JsonObject Disabled(JsonObject root) => root["dshTrayRecovery"] == null ? new JsonObject()
            : root["dshTrayRecovery"] as JsonObject ?? throw new InvalidDataException("暂时停用记录格式无效。");
        void ValidatePaths()
        {
            for (var p = new DirectoryInfo(directory); p != null; p = p.Parent)
                if (p.Exists && (p.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("修复目录不能经过链接或 junction。");
            foreach (string name in new[] { "package.json", "cordis.patch.yml", "pnpm-lock.yaml", ".tray-recovery.lock" })
                if (File.Exists(Path.Combine(directory, name)) && (File.GetAttributes(Path.Combine(directory, name)) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("修复文件不能是链接。");
        }
        string Guard()
        {
            ValidatePaths();
            return string.Join("|", new[] { "package.json", "cordis.patch.yml", "pnpm-lock.yaml" }.Select(name =>
                File.Exists(Path.Combine(directory, name)) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory, name)))) : "missing"));
        }
        internal Snapshot Read(string diagnostic = "")
        {
            var guard = Guard();
            var text = File.ReadAllText(Manifest);
            var root = JsonNode.Parse(text) as JsonObject ?? throw new InvalidDataException("插件清单不是 JSON 对象。");
            var enabled = Bundles(root).Select(n => n?.GetValue<string>() ?? throw new InvalidDataException("空 bundle 名称")).ToList();
            if (enabled.Distinct().Count() != enabled.Count) throw new InvalidDataException("启用清单有重复项，请先手动修复。");
            var disabled = Disabled(root);
            if (disabled.Any(p => enabled.Contains(p.Key) || Core(p.Key) || p.Value is not JsonValue || !p.Value.AsValue().TryGetValue<int>(out int index) || index < 0))
                throw new InvalidDataException("暂时停用记录存在冲突，未做修改。");
            var rows = enabled.Where(n => !Core(n)).Select(n => new Row(n, true, Evidence(n, diagnostic)))
                .Concat(disabled.Select(p => new Row(p.Key, false, Evidence(p.Key, diagnostic)))).ToArray();
            if (Guard() != guard) throw new IOException("读取时配置发生变化，请刷新。");
            return new Snapshot(text, guard, rows);
        }
        internal static string Evidence(string name, string diagnostic)
        {
            // Match explicit package-level diagnostics only, not stack trace mentions or unrelated warnings.
            foreach (var line in diagnostic.Split('\n'))
            {
                if ((line.Contains("Plugin " + name + "@", StringComparison.Ordinal) && line.Contains(" is incompatible with dsh ", StringComparison.Ordinal))
                    || line.Trim().Equals("Error: Cannot find package '" + name + "'", StringComparison.Ordinal))
                    return "明确诊断候选（需人工确认）：" + Program.RedactDiagnostic(line.Trim());
            }
            return "原因未确定；不会自动停用";
        }
        internal string Apply(Snapshot expected, string name, bool enable)
        {
            ValidatePaths();
            using var gate = new FileStream(Path.Combine(directory, ".tray-recovery.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if (Guard() != expected.Guard) throw new IOException("配置已被其他操作更改，请刷新后重新选择。");
            var row = expected.Rows.SingleOrDefault(r => r.Name == name) ?? throw new InvalidOperationException("未找到所选第三方插件。");
            if (row.Enabled == enable) throw new InvalidOperationException("插件状态已改变，请刷新。");
            var root = JsonNode.Parse(expected.Text).AsObject();
            var bundles = Bundles(root);
            var disabled = Disabled(root);
            if (root["dshTrayRecovery"] == null) root["dshTrayRecovery"] = disabled;
            if (enable)
            {
                int originalIndex = disabled[name].GetValue<int>();
                int index = Math.Clamp(originalIndex - disabled.Count(p => p.Value.GetValue<int>() < originalIndex), 0, bundles.Count);
                bundles.Insert(index, JsonValue.Create(name));
                disabled.Remove(name);
            }
            else
            {
                int index = bundles.Select(n => n.GetValue<string>()).ToList().IndexOf(name);
                if (index < 0) throw new InvalidOperationException("启用清单已变化。");
                int originalIndex = index;
                foreach (int missing in disabled.Select(p => p.Value.GetValue<int>()).OrderBy(n => n))
                    if (missing <= originalIndex) originalIndex++;
                disabled[name] = originalIndex;
                bundles.RemoveAt(index);
            }
            string id = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") + "-" + Guid.NewGuid().ToString("N");
            string backup = Manifest + ".tray-backup-" + id;
            string temp = Manifest + ".tray-temp-" + id;
            try
            {
                File.WriteAllText(temp, root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
                if (Guard() != expected.Guard) throw new IOException("保存前配置已变化，操作取消。");
                File.Replace(temp, Manifest, backup); // Same-volume atomic replacement with byte-for-byte backup.
                return backup;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }
}
