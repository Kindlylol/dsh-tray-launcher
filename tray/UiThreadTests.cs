using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DshTray
{
    static partial class Program
    {
        static void RunRepairMenuTests(string directory)
        {
            Directory.CreateDirectory(directory);
            _testHome = Path.Combine(directory, "home");
            _dataDir = directory;
            CreateRepairFixture(_testHome);
            _tray = new NotifyIcon();
            BuildMenu();
            OpenPluginRecovery();
            bool accessible = _menu.Enabled && _menu.Items.OfType<ToolStripMenuItem>().Where(i => i.Text == "查看日志" || i.Text.StartsWith("插件修复") || i.Text == "打开 DSH 主界面").All(i => i.Enabled);
            bool conflictsBlocked = _menu.Items.OfType<ToolStripMenuItem>().Where(i => Equals(i.Tag, "repair-conflict")).All(i => !i.Enabled);
            _repairWindow.WindowState = FormWindowState.Minimized;
            OpenPluginRecovery();
            bool restored = _repairWindow.Visible && _repairWindow.WindowState == FormWindowState.Normal;
            _repairWindow.Close();
            bool unlocked = _menu.Enabled && _menu.Items.OfType<ToolStripMenuItem>().Where(i => Equals(i.Tag, "repair-conflict")).All(i => i.Enabled);
            bool passed = accessible && conflictsBlocked && restored && unlocked;
            File.WriteAllText(Path.Combine(directory, "result.json"), JsonSerializer.Serialize(new { passed, accessible, conflictsBlocked, restored, unlocked }));
            _tray.Dispose();
            Environment.Exit(passed ? 0 : 1);
        }

        static void RunUiThreadTests(string directory)
        {
            Directory.CreateDirectory(directory);
            int uiThread = Environment.CurrentManagedThreadId;
            _tray = new NotifyIcon();
            BuildMenu();
            async void Check()
            {
                bool contextInstalled = SynchronizationContext.Current is WindowsFormsSynchronizationContext;
                await Task.Delay(80);
                bool continuationOnUi = Environment.CurrentManagedThreadId == uiThread;
                File.WriteAllText(Path.Combine(directory, "result.json"), JsonSerializer.Serialize(new { contextInstalled, continuationOnUi, passed = contextInstalled && continuationOnUi }));
                Environment.Exit(contextInstalled && continuationOnUi ? 0 : 1);
            }
            Check();
            Application.Run();
        }
    }
}
