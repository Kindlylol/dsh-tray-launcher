using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DshTray
{
    internal sealed class PluginRecoveryForm : Form
    {
        readonly PluginRecoveryStore store;
        readonly Func<bool> stop;
        readonly Func<Task<string>> retry;
        readonly Func<string> diagnostic;
        PluginRecoveryStore.Snapshot snapshot;
        readonly ListView list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, AccessibleName = "第三方插件列表" };
        readonly TextBox detail = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, AccessibleName = "诊断与操作结果" };
        readonly Button disable = new Button { Text = "停用所选并重试", AutoSize = true, Enabled = false };
        readonly Button enable = new Button { Text = "重新启用并重试", AutoSize = true, Enabled = false };
        readonly Button refresh = new Button { Text = "刷新", AutoSize = true };
        readonly Button start = new Button { Text = "启动 Beta 插件环境", AutoSize = true };
        bool busy;
        internal PluginRecoveryForm(string home, Func<bool> stop, Func<Task<string>> retry, Func<string> diagnostic)
        {
            this.store = new PluginRecoveryStore(home); this.stop = stop; this.retry = retry; this.diagnostic = diagnostic;
            Text = "DSH Beta — 插件修复";
            if (Program.UseOriginalEnvironment) { Text += "（原环境）"; start.Text = "启动原环境插件模式"; }
            Font = new Font("Microsoft YaHei UI", 10);
            AutoScaleMode = AutoScaleMode.Dpi;
            Size = new Size(940, 620); MinimumSize = new Size(760, 500); StartPosition = FormStartPosition.CenterScreen;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(16) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = (Program.UseOriginalEnvironment ? "正在使用原环境，操作将修改原来的 web 插件清单。" : "仅修改 Beta 的 web 插件清单；不卸载软件包、不改补丁。") + "\n请选择一个插件。诊断只是候选证据，不会自动判定或停用依赖。\n数据目录：" + home, AutoEllipsis = true }, 0, 0);
            list.Columns.Add("插件", 300); list.Columns.Add("状态", 100); list.Columns.Add("诊断", 430);
            layout.Controls.Add(list, 0, 1); layout.Controls.Add(detail, 0, 2);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 6, 0, 0) };
            actions.Controls.AddRange(new Control[] { disable, enable, refresh, start });
            layout.Controls.Add(actions, 0, 3); Controls.Add(layout);
            list.SelectedIndexChanged += (_, _) => UpdateButtons();
            refresh.Click += (_, _) => Reload();
            disable.Click += async (_, _) => await Change(false);
            enable.Click += async (_, _) => await Change(true);
            start.Click += async (_, _) =>
            {
                if (busy) return;
                busy = true; refresh.Enabled = start.Enabled = false; UpdateButtons();
                string result;
                try { result = await Task.Run(stop) ? await retry() : "Beta 服务无法安全停止，未重启。"; }
                catch (Exception ex) { result = "启动失败：" + ex.Message; }
                finally { busy = false; refresh.Enabled = start.Enabled = true; }
                Reload(); detail.Text = result + Environment.NewLine + detail.Text;
            };
            FormClosing += (_, e) => { if (busy) e.Cancel = true; };
            Shown += (_, _) => Reload();
        }
        void UpdateButtons()
        {
            var row = list.SelectedItems.Count == 1 ? list.SelectedItems[0].Tag as PluginRecoveryStore.Row : null;
            disable.Enabled = !busy && row?.Enabled == true;
            enable.Enabled = !busy && row?.Enabled == false;
        }
        void Reload()
        {
            list.Items.Clear(); snapshot = null;
            try
            {
                string log = diagnostic(); snapshot = store.Read(log);
                foreach (var row in snapshot.Rows)
                    list.Items.Add(new ListViewItem(new[] { row.Name, row.Enabled ? "已选择启用" : "暂时停用", row.Evidence }) { Tag = row });
                detail.Text = snapshot.Rows.Count == 0 ? "当前环境尚无第三方插件。" : "请选择插件后操作。启用清单不代表插件功能已验收。";
                if (!string.IsNullOrWhiteSpace(log)) detail.AppendText(Environment.NewLine + Environment.NewLine + Program.RedactDiagnostic(log));
            }
            catch (Exception ex) { detail.Text = "无法读取插件清单：" + ex.Message; }
            UpdateButtons();
        }
        async Task Change(bool enabled)
        {
            if (busy || snapshot == null || list.SelectedItems.Count != 1) return;
            var row = (PluginRecoveryStore.Row)list.SelectedItems[0].Tag;
            if (MessageBox.Show(this, (enabled ? "重新启用 " : "暂时停用 ") + row.Name + "？\n将停止当前后台、备份清单并尝试一次插件启动；失败回到核心。" + (Program.UseOriginalEnvironment ? "本次会修改原环境。" : "不会更改正式环境。"), "确认插件操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            busy = true; refresh.Enabled = start.Enabled = false; UpdateButtons();
            string result;
            try
            {
                if (!await Task.Run(stop)) throw new IOException("无法安全停止 Beta 服务，未修改配置。");
                string backup = store.Apply(snapshot, row.Name, enabled);
                result = "清单已备份：" + backup + Environment.NewLine + await retry();
            }
            catch (Exception ex) { result = "操作未完成：" + ex.Message + "\n不会自动反复尝试；请刷新状态。"; }
            finally { busy = false; refresh.Enabled = start.Enabled = true; }
            Reload(); detail.Text = result + Environment.NewLine + detail.Text;
        }
    }
}
