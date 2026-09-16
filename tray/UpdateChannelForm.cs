using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DshTray
{
    static partial class Program
    {
        sealed class UpdateChannelForm : Form
        {
            internal RemoteVersionInfo SelectedVersion { get; private set; }
            internal readonly List<RadioButton> Choices = new List<RadioButton>();
            internal readonly Button InstallButton;

            internal UpdateChannelForm(string local, List<RemoteVersionInfo> versions)
            {
                Text = "检测 DSH 更新 — 选择渠道";
                Font = new Font("Microsoft YaHei UI", 10);
                AutoScaleMode = AutoScaleMode.Dpi;
                ClientSize = new Size(580, 280);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterScreen;
                MaximizeBox = false;
                MinimizeBox = false;
                Controls.Add(new Label { Text = "当前 DSH：" + local, AutoSize = true, Location = new Point(20, 20) });
                InstallButton = new Button { Text = "更新所选版本", Location = new Point(320, 225), Size = new Size(140, 34), Enabled = false };
                var cancel = new Button { Text = "取消", Location = new Point(470, 225), Size = new Size(90, 34), DialogResult = DialogResult.Cancel };
                for (int i = 0; i < versions.Count; i++)
                {
                    var version = versions[i];
                    bool available = CanUpdate(local, version);
                    string status = !version.Installable ? "渠道未发布或版本不可用"
                        : !System.Text.RegularExpressions.Regex.IsMatch(local ?? "", @"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$") ? "本地版本未知，无法安全更新"
                        : available ? "可更新" : CompareVersions(version.Version, local) == 0 ? "与本地相同" : "不高于本地，不降级";
                    var option = new RadioButton {
                        Text = version.Source + "：" + (version.Version ?? "未提供") + "  ·  " + status,
                        Location = new Point(20, 62 + i * 42), Size = new Size(540, 32), Enabled = available,
                        AccessibleName = version.Source + " 更新渠道"
                    };
                    option.CheckedChanged += (s, e) => { if (option.Checked) { SelectedVersion = version; InstallButton.Enabled = true; } };
                    Choices.Add(option);
                    Controls.Add(option);
                }
                Controls.Add(new Label {
                    Text = "Alpha 为测试版，可能与插件不兼容。\n建议先切换核心模式检测更新；更新并启动成功后，再尝试插件模式。",
                    Location = new Point(20, 155), Size = new Size(540, 58)
                });
                InstallButton.Click += (s, e) => { if (SelectedVersion != null) DialogResult = DialogResult.OK; };
                Controls.Add(InstallButton);
                Controls.Add(cancel);
                CancelButton = cancel;
                // Deliberate selection: no default Alpha choice or Enter-to-install.
            }
        }
    }
}
