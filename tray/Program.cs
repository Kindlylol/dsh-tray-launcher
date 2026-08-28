using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DshTray
{
    static class Program
    {
        const string WHALE_PATH = "M48.8354 10.0479C48.3232 9.79199 48.1025 10.2798 47.8032 10.5278C47.7007 10.6079 47.6143 10.7119 47.5273 10.8076C46.7793 11.624 45.9048 12.1597 44.7622 12.0957C43.0923 12 41.666 12.5356 40.4058 13.8398C40.1377 12.2319 39.2476 11.272 37.8926 10.6558C37.1836 10.3359 36.4668 10.0156 35.9702 9.31982C35.6235 8.82373 35.5293 8.27197 35.356 7.72754C35.2456 7.3999 35.1353 7.06396 34.7651 7.00781C34.3633 6.94385 34.2056 7.2876 34.0479 7.57568C33.418 8.75195 33.1733 10.0479 33.1973 11.3599C33.2524 14.312 34.4736 16.6641 36.8999 18.3359C37.1758 18.5278 37.2466 18.7197 37.1597 19C36.9946 19.5757 36.7974 20.1357 36.624 20.7119C36.5137 21.0801 36.3486 21.1597 35.9624 21C34.6309 20.4321 33.481 19.5918 32.4644 18.5757C30.7393 16.8721 29.1792 14.9917 27.2334 13.52C26.7764 13.1758 26.3193 12.856 25.8467 12.5518C23.8618 10.584 26.1069 8.96777 26.627 8.77588C27.1704 8.57568 26.8159 7.8877 25.0591 7.896C23.3022 7.90381 21.6953 8.50391 19.647 9.30371C19.3477 9.42383 19.0322 9.51172 18.7095 9.58398C16.8501 9.22363 14.9199 9.14355 12.9033 9.37598C9.10596 9.80762 6.07275 11.6396 3.84326 14.7681C1.16455 18.5278 0.53418 22.7998 1.30664 27.2559C2.11768 31.9521 4.46582 35.8398 8.07373 38.8799C11.8159 42.0322 16.1255 43.5762 21.041 43.2803C24.0269 43.104 27.3516 42.6963 31.1016 39.4561C32.0469 39.936 33.0396 40.1279 34.686 40.272C35.9546 40.3921 37.1758 40.208 38.1211 40.0078C39.6021 39.688 39.4995 38.2881 38.9639 38.0322C34.623 35.9678 35.5762 36.8081 34.71 36.1279C36.9155 33.4639 40.2402 30.6958 41.54 21.728C41.6426 21.0161 41.5557 20.5679 41.54 19.9917C41.5322 19.6396 41.6108 19.5039 42.0049 19.4639C43.0923 19.3359 44.1479 19.0317 45.1167 18.4878C47.9292 16.9199 49.064 14.3438 49.3315 11.2559C49.3711 10.7837 49.3237 10.2959 48.8354 10.0479ZM24.3262 37.8398C20.1196 34.4639 18.0791 33.3521 17.2358 33.3999C16.4482 33.4482 16.5898 34.3682 16.7632 34.9678C16.9443 35.5601 17.1812 35.9683 17.5117 36.4878C17.7402 36.832 17.8979 37.3442 17.2832 37.728C15.9282 38.584 13.5728 37.4399 13.4624 37.3838C10.7207 35.7358 8.42822 33.5601 6.81348 30.584C5.25342 27.7197 4.34766 24.6479 4.19775 21.3677C4.1582 20.5757 4.38672 20.2959 5.15869 20.1519C6.17529 19.96 7.22314 19.9199 8.23926 20.0718C12.5327 20.7119 16.1885 22.6719 19.2529 25.7759C21.002 27.5439 22.3252 29.6558 23.6885 31.7202C25.1377 33.9121 26.6978 36 28.6831 37.7119C29.3843 38.312 29.9434 38.7681 30.479 39.104C28.8643 39.2881 26.1699 39.3281 24.3262 37.8398ZM26.3433 24.6001C26.3433 24.248 26.6191 23.9678 26.9658 23.9678C27.0444 23.9678 27.1152 23.9839 27.1782 24.0078C27.2651 24.04 27.3438 24.0879 27.4067 24.1602C27.5171 24.272 27.5801 24.4321 27.5801 24.6001C27.5801 24.9521 27.3042 25.2319 26.9575 25.2319C26.6108 25.2319 26.3433 24.9521 26.3433 24.6001ZM32.6064 27.8799C32.2046 28.0479 31.8027 28.1919 31.4165 28.208C30.8179 28.2397 30.1641 27.9922 29.8096 27.688C29.2583 27.2158 28.8643 26.9521 28.6987 26.1279C28.6279 25.7759 28.6675 25.2319 28.7305 24.9199C28.8721 24.248 28.7144 23.8159 28.2495 23.4238C27.8716 23.104 27.3911 23.0161 26.8633 23.0161C26.666 23.0161 26.4849 22.9277 26.3511 22.856C26.1304 22.7441 25.9492 22.4639 26.1226 22.1201C26.1777 22.0078 26.4458 21.7358 26.5088 21.688C27.2256 21.272 28.0527 21.4077 28.8169 21.7197C29.5259 22.0161 30.0615 22.5601 30.834 23.3281C31.6216 24.2559 31.7632 24.5117 32.2124 25.208C32.5669 25.752 32.8901 26.312 33.1104 26.9521C33.2446 27.3521 33.0713 27.6802 32.6064 27.8799Z";

        static NotifyIcon _tray;
        static ContextMenuStrip _menu;
        static string _logPath;
        static string _exeDir;
        static string _dataDir;
        static ToolStripMenuItem _statusItem;
        static ToolStripMenuItem _updateItem;
        static readonly object _serviceStateLock = new object();
        static readonly object _serviceLogLock = new object();
        static Mutex _trayMutex;
        static Process _managedServiceProcess;

        static string LocalPackagePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "npm", "node_modules", "@deepseek-ai", "dsh", "package.json");
        static string DshCliPath => Path.Combine(Path.GetDirectoryName(LocalPackagePath), "lib", "bin.js");
        const string GITHUB_REPO = "https://github.com/deepseek-ai/deepseek-harness";
        static string DshHome => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dsh");
        const int PORT = 3080;
        static string ServiceStatePath => Path.Combine(_dataDir ?? AppContext.BaseDirectory, "dsh-service.state");
        static bool _startupStarted;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);

        static bool _silent = false;

        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            if (args.Length > 0 && args[0] == "--gen-ico")
            {
                GenIco(Path.Combine(AppContext.BaseDirectory, "whale.ico"), RenderAppIcon,
                    Path.Combine(AppContext.BaseDirectory, "whale-preview.png"));
                GenIco(Path.Combine(AppContext.BaseDirectory, "whale-blue.ico"), RenderLegacyIcon,
                    Path.Combine(AppContext.BaseDirectory, "whale-blue-preview.png"));
                return;
            }
            if (args.Length > 0 && args[0] == "--selftest")
            {
                RunSelfTest();
                return;
            }
            if (args.Length > 0 && args[0] == "--test-restart")
            {
                _silent = true;
                _exeDir = Path.GetDirectoryName(Application.ExecutablePath);
                InitDataPaths();
                RestartService();
                return;
            }
            if (args.Length > 0 && args[0] == "--test-stop")
            {
                _silent = true;
                _exeDir = Path.GetDirectoryName(Application.ExecutablePath);
                InitDataPaths();
                StopService();
                Log("stop test done, running=" + IsServiceRunning());
                return;
            }
            if (args.Length > 0 && args[0] == "--test-update")
            {
                _silent = true;
                _exeDir = Path.GetDirectoryName(Application.ExecutablePath);
                InitDataPaths();
                string local = GetLocalVersion();
                RemoteVersionInfo remoteInfo = GetRemoteVersionAsync().GetAwaiter().GetResult();
                Log("update check: local=" + local + " remote=" + (remoteInfo?.Version ?? "null") + " installable=" + (remoteInfo?.Installable ?? false));
                return;
            }

            _exeDir = Path.GetDirectoryName(Application.ExecutablePath);
            InitDataPaths();

            // Redirect unhandled exceptions to log
            Application.ThreadException += (s, e) => Log("unhandled: " + e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Log("unhandled app: " + e.ExceptionObject);

            _trayMutex = new Mutex(true, "Local\\DeepSeekHarness.DshTray", out bool ownsMutex);
            if (!ownsMutex)
            {
                Log("another tray instance is already running");
                _trayMutex.Dispose();
                return;
            }

            Log("tray starting, exe=" + _exeDir);

            _tray = new NotifyIcon();
            _tray.Icon = MakeIcon();
            _tray.Text = "DeepSeek Harness";
            _tray.Visible = true;

            // Left-click: no action (do nothing). Double-click: open UI.
            _tray.MouseClick += (s, e) => { /* single click: intentionally no-op */ };
            _tray.DoubleClick += (s, e) => OpenBrowser();

            BuildMenu();

            Application.Idle += StartupOnce;
            UpdateStatusAsync();

            Application.Run();
            _trayMutex.ReleaseMutex();
            _trayMutex.Dispose();
        }

        static void InitDataPaths()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _dataDir = Path.Combine(root, "DSH Tray Launcher");
            Directory.CreateDirectory(_dataDir);
            _logPath = Path.Combine(_dataDir, "dsh-tray.log");
        }

        static async void StartupOnce(object sender, EventArgs e)
        {
            if (_startupStarted) return;
            _startupStarted = true;
            Application.Idle -= StartupOnce;
            bool running = await Task.Run(IsServiceRunning);
            if (running)
            {
                Log("service already running on startup");
                return;
            }
            Log("service not running on startup, starting...");
            await Task.Run(StartService);
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(1000);
                if (await Task.Run(IsServiceRunning))
                {
                    Log("service started on startup");
                    OpenBrowser();
                    return;
                }
            }
            Log("service failed to start on startup");
        }

        static void BuildMenu()
        {
            _menu = new ContextMenuStrip();
            // System renderer: follows Windows 11 theme, native speed, full-row hit area
            _menu.Renderer = new ToolStripSystemRenderer();

            _statusItem = new ToolStripMenuItem("服务: 检测中...") { Enabled = false };
            _menu.Items.Add(_statusItem);
            _menu.Items.Add(new ToolStripSeparator());

            var open = new ToolStripMenuItem("打开 DSH 主界面");
            open.Click += (s, e) => OpenBrowser();
            _menu.Items.Add(open);

            var restart = new ToolStripMenuItem("重启 DSH 服务");
            restart.Click += (s, e) => RestartService();
            _menu.Items.Add(restart);

            _updateItem = new ToolStripMenuItem("检测更新");
            _updateItem.Click += (s, e) => CheckForUpdatesAsync();
            _menu.Items.Add(_updateItem);

            var logs = new ToolStripMenuItem("查看日志");
            logs.Click += (s, e) => OpenLogs();
            _menu.Items.Add(logs);

            var autostart = new ToolStripMenuItem("开机自启");
            autostart.Click += (s, e) => ToggleAutostart();
            _menu.Items.Add(autostart);

            var desktop = new ToolStripMenuItem("创建桌面图标");
            desktop.Click += (s, e) => CreateDesktopShortcut();
            _menu.Items.Add(desktop);

            _menu.Items.Add(new ToolStripSeparator());

            var exit = new ToolStripMenuItem("退出");
            exit.Click += (s, e) => ExitApp();
            _menu.Items.Add(exit);

            _tray.ContextMenuStrip = _menu;
        }

        // ---------- Icon ----------
        static Bitmap RenderAppIcon(int size)
        {
            return RenderEmbeddedIcon(size, "DshTray.deepseek-harness-icon-source.png", true);
        }

        static Bitmap RenderLegacyIcon(int size)
        {
            return RenderEmbeddedIcon(size, "DshTray.deepseek-icon-source.png", false);
        }

        static Bitmap RenderEmbeddedIcon(int size, string resourceName, bool opticalCenter)
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException("内置图标资源缺失"))
            using (var source = new Bitmap(stream))
            {
                var result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(result))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    int margin = opticalCenter ? (int)Math.Round(size * 0.078125) : 0;
                    int shiftX = opticalCenter ? (int)Math.Round(size * 0.0234375) : 0;
                    int shiftY = opticalCenter ? (int)Math.Round(size * 0.01953125) : 0;
                    g.DrawImage(source, new Rectangle(
                        margin + shiftX,
                        margin + shiftY,
                        size - margin * 2,
                        size - margin * 2));
                }
                return result;
            }
        }

        static Rectangle GetInkBounds(Bitmap bitmap)
        {
            int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    if ((c.R + c.G + c.B) / 3 < 245)
                    {
                        left = Math.Min(left, x); top = Math.Min(top, y);
                        right = Math.Max(right, x); bottom = Math.Max(bottom, y);
                    }
                }
            return right < left ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        static PointF GetInkCentroid(Bitmap bitmap)
        {
            double sumX = 0, sumY = 0, weightSum = 0;
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    double weight = 255 - (c.R + c.G + c.B) / 3.0;
                    if (weight <= 10) continue;
                    sumX += x * weight;
                    sumY += y * weight;
                    weightSum += weight;
                }
            return weightSum == 0 ? PointF.Empty : new PointF((float)(sumX / weightSum), (float)(sumY / weightSum));
        }

        static void GenIco(string path, Func<int, Bitmap> renderer, string previewPath)
        {
            int[] sizes = { 16, 24, 32, 48, 64, 128, 256 };
            var frames = new List<Bitmap>();
            foreach (var s in sizes)
            {
                var bmp = renderer(s);
                frames.Add(bmp);
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((ushort)0);          // reserved
                bw.Write((ushort)1);          // type: icon
                bw.Write((ushort)frames.Count);
                int offset = 6 + 16 * frames.Count;
                var pngs = new List<byte[]>();
                foreach (var b in frames)
                {
                    int w = b.Width >= 256 ? 0 : b.Width;
                    int h = b.Height >= 256 ? 0 : b.Height;
                    bw.Write((byte)w);
                    bw.Write((byte)h);
                    bw.Write((byte)0); // palette
                    bw.Write((byte)0); // reserved
                    bw.Write((ushort)1); // planes
                    bw.Write((ushort)32); // bpp
                    byte[] png;
                    using (var pms = new MemoryStream())
                    {
                        b.Save(pms, ImageFormat.Png);
                        png = pms.ToArray();
                    }
                    pngs.Add(png);
                    bw.Write((uint)png.Length);
                    bw.Write((uint)offset);
                    offset += png.Length;
                }
                foreach (var p in pngs) bw.Write(p);
                bw.Flush();
                File.WriteAllBytes(path, ms.ToArray());
            }
            foreach (var b in frames) b.Dispose();
            using (var big = renderer(256))
            {
                big.Save(previewPath, ImageFormat.Png);
            }
            Log("icon written: " + path);
        }

        static Icon MakeIcon()
        {
            try
            {
                using (var bmp = RenderAppIcon(32))
                {
                    IntPtr hIcon = bmp.GetHicon();
                    try
                    {
                        using (var borrowed = Icon.FromHandle(hIcon))
                            return (Icon)borrowed.Clone();
                    }
                    finally { DestroyIcon(hIcon); }
                }
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        static void RunSelfTest()
        {
            var sb = new StringBuilder();
            bool iconChecksOk = true;
            sb.AppendLine("=== dsh-tray selftest ===");

            // 1. icon
            try
            {
                using (var icon = MakeIcon())
                {
                    sb.AppendLine("icon: OK (size " + icon.Width + "x" + icon.Height + ")");
                }
                foreach (int size in new[] { 16, 24, 32, 48, 64, 128, 256 })
                    using (var bitmap = RenderAppIcon(size))
                    {
                        Rectangle b = GetInkBounds(bitmap);
                        PointF centroid = GetInkCentroid(bitmap);
                        float center = (size - 1) / 2f;
                        float dx = Math.Abs(centroid.X - center);
                        float dy = Math.Abs(centroid.Y - center);
                        bool geometryOk = !b.IsEmpty
                            && b.Width >= size * 0.75 && b.Width <= size * 0.9
                            && b.Height >= size * 0.5 && b.Height <= size * 0.7
                            && dx <= Math.Max(1.5f, size * 0.025f)
                            && dy <= Math.Max(1.5f, size * 0.025f);
                        iconChecksOk &= geometryOk;
                        sb.AppendLine("icon " + size + ": ink=" + b.X + "," + b.Y + " " + b.Width + "x" + b.Height
                            + " centroid-error=" + dx.ToString("0.0", CultureInfo.InvariantCulture) + "," + dy.ToString("0.0", CultureInfo.InvariantCulture)
                            + " " + (geometryOk ? "OK" : "FAIL"));
                    }
                using (var legacy = RenderLegacyIcon(32))
                {
                    bool legacyOk = !GetInkBounds(legacy).IsEmpty;
                    iconChecksOk &= legacyOk;
                    sb.AppendLine("legacy blue icon: " + (legacyOk ? "OK" : "FAIL"));
                }
            }
            catch (Exception ex)
            {
                iconChecksOk = false;
                sb.AppendLine("icon: FAIL - " + ex.Message);
            }

            // 2. local version
            sb.AppendLine("local version: " + GetLocalVersion());

            // 3. remote version
            try
            {
                RemoteVersionInfo r = GetRemoteVersionAsync().GetAwaiter().GetResult();
                sb.AppendLine("remote version: " + (r?.Version ?? "(null)") + " (installable=" + (r?.Installable ?? false) + ", source=" + (r?.Source ?? "none") + ")");
            }
            catch (Exception ex) { sb.AppendLine("remote version: FAIL - " + ex.Message); }

            // 4. port check
            sb.AppendLine("service running: " + IsServiceRunning());

            // 5. SemVer ordering used by update detection
            bool semverOk = CompareVersions("0.1.1-rc.2", "0.1.1") < 0
                && CompareVersions("0.1.1-rc.10", "0.1.1-rc.2") > 0
                && CompareVersions("0.1.2", "0.1.1") > 0;
            sb.AppendLine("semver: " + (semverOk ? "OK" : "FAIL"));

            sb.AppendLine("=== end ===");
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "dsh-tray-selftest.txt"), sb.ToString());
            if (!iconChecksOk) Environment.ExitCode = 1;
        }

        // ---------- Logging ----------
        static void Log(string msg)
        {
            try
            {
                File.AppendAllText(_logPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + msg + Environment.NewLine);
            }
            catch { }
        }

        static DialogResult Msg(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (_silent)
            {
                Log("[silent] " + title + ": " + text);
                return DialogResult.No; // never trigger side effects in silent test mode
            }
            return MessageBox.Show(text, title, buttons, icon);
        }

        // ---------- Service control ----------
        static bool IsServiceRunning()
        {
            try
            {
                int pid;
                DateTime started;
                if (!TryReadManagedService(out pid, out started)) return false;
                using (var managed = Process.GetProcessById(pid))
                {
                    if (managed.StartTime != started) return false;
                }
                return IsPortOpen();
            }
            catch { }
            return false;
        }

        static void OpenBrowser()
        {
            try { Process.Start(new ProcessStartInfo("http://127.0.0.1:" + PORT) { UseShellExecute = true }); }
            catch (Exception ex) { Log("open browser: " + ex.Message); }
        }

        static void RestartService()
        {
            Log("restart requested");
            try
            {
                StopService();
                Thread.Sleep(1500);
                StartService();
                // Wait up to 40s for the service
                bool ok = false;
                for (int i = 0; i < 40; i++)
                {
                    Thread.Sleep(1000);
                    if (IsServiceRunning()) { ok = true; break; }
                }
                if (ok)
                {
                    Log("restart OK");
                    Msg("DSH 服务重启成功\nhttp://127.0.0.1:" + PORT,
                        "DeepSeek Harness", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Log("restart FAILED");
                    Msg("DSH 服务启动失败，请查看日志。\n日志: " + _logPath,
                        "DeepSeek Harness", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log("restart error: " + ex);
                Msg("重启出错: " + ex.Message, "DeepSeek Harness",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateStatusAsync();
            }
        }

        static bool StopService()
        {
            // Only stop the exact process tree started by this tray. Never kill an
            // unrelated listener that happens to use the same port.
            try
            {
                int pid;
                DateTime started;
                if (!TryReadManagedService(out pid, out started))
                {
                    Log("stop skipped: no managed DSH process");
                    return !IsPortOpen();
                }
                using (var managed = Process.GetProcessById(pid))
                {
                    if (managed.StartTime != started) { Log("stop skipped: managed PID was reused"); return false; }
                }
                var psi = new ProcessStartInfo("taskkill", "/PID " + pid + " /T /F")
                { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                using (var p = Process.Start(psi)) { p.WaitForExit(10000); }
                Log("stopped managed pid " + pid);
                DeleteServiceState();
                return !IsPortOpen();
            }
            catch (Exception ex) { Log("stop: " + ex.Message); return false; }
        }

        static void StartService()
        {
            try
            {
                if (IsPortOpen() && !IsServiceRunning())
                    throw new InvalidOperationException("端口 " + PORT + " 已被非受管进程占用，已拒绝启动 DSH");
                if (!File.Exists(DshCliPath))
                    throw new FileNotFoundException("未找到 DSH CLI", DshCliPath);
                var logFile = Path.Combine(_dataDir ?? AppContext.BaseDirectory, "dsh-service.log");
                var psi = new ProcessStartInfo("node")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = DshHome
                };
                psi.ArgumentList.Add(DshCliPath);
                psi.ArgumentList.Add("web");
                psi.ArgumentList.Add("--host");
                psi.ArgumentList.Add("127.0.0.1");
                psi.ArgumentList.Add("--port");
                psi.ArgumentList.Add(PORT.ToString(CultureInfo.InvariantCulture));
                psi.ArgumentList.Add("--no-open");
                var proc = Process.Start(psi);
                if (proc == null) throw new InvalidOperationException("无法启动 dsh 进程");
                proc.OutputDataReceived += (s, e) => AppendServiceLog(logFile, e.Data);
                proc.ErrorDataReceived += (s, e) => AppendServiceLog(logFile, e.Data);
                proc.EnableRaisingEvents = true;
                proc.Exited += (s, e) => Log("managed dsh process exited pid=" + proc.Id + " code=" + proc.ExitCode);
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                _managedServiceProcess = proc;
                WriteServiceState(proc);
                Log("started managed dsh web pid=" + proc.Id);
            }
            catch (Exception ex) { Log("start: " + ex.Message); }
        }

        static void AppendServiceLog(string path, string line)
        {
            if (line == null) return;
            try
            {
                lock (_serviceLogLock)
                    File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        static bool IsPortOpen()
        {
            try
            {
                using (var c = new System.Net.Sockets.TcpClient())
                {
                    var ar = c.BeginConnect("127.0.0.1", PORT, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(800)) return false;
                    c.EndConnect(ar);
                    return true;
                }
            }
            catch { return false; }
        }

        static void WriteServiceState(Process process)
        {
            lock (_serviceStateLock)
            {
                File.WriteAllText(ServiceStatePath, process.Id + "|" + process.StartTime.Ticks.ToString(CultureInfo.InvariantCulture));
            }
        }

        static bool TryReadManagedService(out int pid, out DateTime started)
        {
            pid = 0; started = DateTime.MinValue;
            try
            {
                string[] parts = File.ReadAllText(ServiceStatePath).Trim().Split('|');
                if (parts.Length != 2 || !int.TryParse(parts[0], out pid)) return false;
                long ticks;
                if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks)) return false;
                started = new DateTime(ticks);
                using (var p = Process.GetProcessById(pid)) { return p.StartTime == started; }
            }
            catch { pid = 0; started = DateTime.MinValue; return false; }
        }

        static void DeleteServiceState()
        {
            try { if (File.Exists(ServiceStatePath)) File.Delete(ServiceStatePath); } catch { }
        }

        // ---------- Update ----------
        static async void CheckForUpdatesAsync()
        {
            _updateItem.Enabled = false;
            _updateItem.Text = "检测中...";
            try
            {
                Log("update check started");
                string local = GetLocalVersion();
                RemoteVersionInfo remoteInfo = await GetRemoteVersionAsync();
                string remote = remoteInfo?.Version;

                if (string.IsNullOrEmpty(remote))
                {
                    Msg("无法获取最新版本（网络或限流问题）\n本地版本: " + local,
                        "检测更新", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _updateItem.Text = "检测更新 (本地 " + local + " / 最新 " + remote + ")";

                if (CompareVersions(local, remote) >= 0)
                {
                    Msg("无需更新\n本地: " + local + "\n最新: " + remote,
                        "检测更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!remoteInfo.Installable)
                {
                    Msg("发现新版本 " + remote + "，但该版本目前只有 Git 标签，尚未发布到 npm。\n\n请等待 npm 发布后再使用托盘自动更新。",
                        "检测更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log("update unavailable: " + remoteInfo.Source);
                    return;
                }

                var r = Msg(
                    "发现新版本！\n本地: " + local + "\n最新: " + remote + "\n\n是否立即更新？",
                    "检测更新", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    PerformUpdate(remote);
                }
            }
            catch (Exception ex)
            {
                Log("update check error: " + ex);
                Msg("检测更新出错: " + ex.Message, "检测更新",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _updateItem.Enabled = true;
            }
        }

        static string GetLocalVersion()
        {
            try
            {
                if (File.Exists(LocalPackagePath))
                {
                    var json = File.ReadAllText(LocalPackagePath);
                    var m = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                    if (m.Success) return m.Groups[1].Value;
                }
                // fallback: run dsh --version (via cmd to resolve .cmd shim)
                var psi = new ProcessStartInfo("cmd.exe", "/c dsh --version")
                {
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    string o = p.StandardOutput.ReadToEnd().Trim();
                    string e = p.StandardError.ReadToEnd().Trim();
                    p.WaitForExit(10000);
                    return (o + e).Trim();
                }
            }
            catch (Exception ex) { Log("local version: " + ex.Message); return "未知"; }
        }

        sealed class RemoteVersionInfo
        {
            public string Version { get; init; }
            public bool Installable { get; init; }
            public string Source { get; init; }
        }

        static async Task<RemoteVersionInfo> GetRemoteVersionAsync()
        {
            RemoteVersionInfo npmInfo = null;
            RemoteVersionInfo gitInfo = null;
            // npm is the only source that the automatic installer can consume.
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(30);
                    string json = await http.GetStringAsync("https://registry.npmmirror.com/@deepseek-ai%2fdsh");
                    var m = Regex.Match(json, "\\\"dist-tags\\\"\\s*:\\s*\\{[^}]*\\\"latest\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
                    if (!m.Success) m = Regex.Match(json, "\\\"version\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
                    if (m.Success)
                    {
                        Log("remote (npm): " + m.Groups[1].Value);
                        npmInfo = new RemoteVersionInfo { Version = m.Groups[1].Value, Installable = true, Source = "npm" };
                    }
                }
            }
            catch (Exception ex) { Log("npm registry: " + ex.Message); }

            // Git tags are useful for visibility, but are not a safe npm install source.
            try
            {
                var psi = new ProcessStartInfo("git", "ls-remote --tags " + GITHUB_REPO)
                {
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p == null) throw new InvalidOperationException("无法启动 git");
                    string o = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(30000);
                    if (p.ExitCode != 0) throw new InvalidOperationException("git 退出码 " + p.ExitCode);
                    var tags = new List<string>();
                    foreach (var line in o.Split('\n'))
                    {
                        var parts = line.Trim().Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && parts[1].StartsWith("refs/tags/dsh-v"))
                            tags.Add(parts[1].Substring("refs/tags/dsh-v".Length));
                    }
                    if (tags.Count > 0)
                    {
                        tags.Sort(CompareVersions);
                        string version = tags[tags.Count - 1];
                        Log("remote (git tag, not installable): " + version);
                        gitInfo = new RemoteVersionInfo { Version = version, Installable = false, Source = "git tag" };
                    }
                }
            }
            catch (Exception ex) { Log("git tags: " + ex.Message); }

            if (npmInfo == null) return gitInfo;
            if (gitInfo == null) return npmInfo;
            return CompareVersions(npmInfo.Version, gitInfo.Version) >= 0 ? npmInfo : gitInfo;
        }

        static int CompareVersions(string a, string b)
        {
            // Normalize: strip any leading 'v', compare numeric parts first,
            // then prerelease suffixes. rc.X < stable; higher rc number > lower.
            string A = a.TrimStart('v');
            string B = b.TrimStart('v');
            var ma = Regex.Match(A, @"^(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.-]+))?");
            var mb = Regex.Match(B, @"^(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.-]+))?");
            if (!ma.Success || !mb.Success) return string.Compare(A, B, StringComparison.OrdinalIgnoreCase);
            for (int i = 1; i <= 3; i++)
            {
                int c = int.Parse(ma.Groups[i].Value, CultureInfo.InvariantCulture).CompareTo(int.Parse(mb.Groups[i].Value, CultureInfo.InvariantCulture));
                if (c != 0) return c;
            }
            bool preA = ma.Groups[4].Success, preB = mb.Groups[4].Success;
            if (preA != preB) return preA ? -1 : 1;
            if (!preA) return 0;
            string[] pa = ma.Groups[4].Value.Split('.');
            string[] pb = mb.Groups[4].Value.Split('.');
            int n = Math.Min(pa.Length, pb.Length);
            for (int i = 0; i < n; i++)
            {
                bool ax = int.TryParse(pa[i], out var x), by = int.TryParse(pb[i], out var y);
                int c = ax && by ? x.CompareTo(y) : ax != by ? (ax ? -1 : 1) : string.Compare(pa[i], pb[i], StringComparison.OrdinalIgnoreCase);
                if (c != 0) return c;
            }
            return pa.Length.CompareTo(pb.Length);
        }

        static void PerformUpdate(string targetVersion)
        {
            Log("update to " + targetVersion + " starting");
            bool restartService = false;
            try
            {
                if (!Regex.IsMatch(targetVersion ?? "", @"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$"))
                    throw new InvalidOperationException("目标版本格式无效: " + targetVersion);

                restartService = IsServiceRunning();
                if (!restartService && IsPortOpen())
                    throw new InvalidOperationException("端口 " + PORT + " 由非受管进程占用，无法安全更新");

                // 1) stop service
                if (restartService && !StopService())
                    throw new InvalidOperationException("无法确认 DSH 服务已安全停止");
                Thread.Sleep(1500);

                // 2) run npm install -g with progress
                var psi = new ProcessStartInfo("cmd.exe")
                {
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true
                };
                psi.ArgumentList.Add("/d");
                psi.ArgumentList.Add("/s");
                psi.ArgumentList.Add("/c");
                psi.ArgumentList.Add("npm install -g @deepseek-ai/dsh@" + targetVersion + " --registry=https://registry.npmmirror.com");
                using (var proc = Process.Start(psi))
                {
                    if (proc == null) throw new InvalidOperationException("无法启动 npm");
                    using (var form = new ProgressForm(proc, TimeSpan.FromMinutes(5)))
                    {
                        form.ShowDialog();
                        if (form.TimedOut) throw new TimeoutException("npm 更新超过 5 分钟，已终止");
                        if (form.Canceled) throw new OperationCanceledException("用户取消了更新");
                    }
                    if (!proc.HasExited) proc.WaitForExit(5000);
                    if (!proc.HasExited) throw new TimeoutException("npm 更新进程未能退出");
                    if (proc.ExitCode != 0) throw new InvalidOperationException("npm 更新失败，退出码 " + proc.ExitCode);
                }

                // 3) verify
                string now = GetLocalVersion();
                if (!string.Equals(now, targetVersion, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("安装后版本不一致，期望 " + targetVersion + "，实际 " + now);
                Log("update done, local now: " + now);
                MessageBox.Show("更新完成！\n新版本: " + now, "DeepSeek Harness",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                Log("update error: " + ex);
                MessageBox.Show("更新失败: " + ex.Message, "DeepSeek Harness",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (restartService && !IsServiceRunning())
                {
                    StartService();
                    Thread.Sleep(2000);
                    Log("service recovery after update: " + IsServiceRunning());
                }
                UpdateStatusAsync();
            }
        }

        // ---------- Desktop shortcut ----------
        static void CreateDesktopShortcut()
        {
            try
            {
                // Resolve the ACTUAL desktop path (handles OneDrive-redirected desktop automatically)
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (string.IsNullOrEmpty(desktop) || !Directory.Exists(desktop))
                {
                    Msg("无法确定桌面路径。\n探测到: " + (desktop ?? "(空)"), "创建桌面图标",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string lnkPath = Path.Combine(desktop, "DeepSeek Harness.lnk");

                // If exists, ask to overwrite
                if (File.Exists(lnkPath))
                {
                    var r = Msg("桌面图标已存在，是否覆盖？", "创建桌面图标",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r != DialogResult.Yes) return;
                    try { File.Delete(lnkPath); } catch { }
                }

                // Create .lnk via WScript.Shell COM
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(lnkPath);
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                shortcut.IconLocation = Application.ExecutablePath + ",0";
                shortcut.Description = "DeepSeek Harness";
                shortcut.Save();
                SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);

                Log("desktop shortcut created: " + lnkPath);
                Msg("桌面图标已创建：\n" + lnkPath + "\n\n(已自动识别桌面路径，OneDrive 同步桌面同样适用)",
                    "创建桌面图标", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("desktop shortcut error: " + ex);
                Msg("创建桌面图标失败: " + ex.Message, "创建桌面图标",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- Autostart ----------
        static void ToggleAutostart()
        {
            try
            {
                const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                {
                    string name = "DSHTray";
                    object existing = key.GetValue(name);
                    string exe = "\"" + Application.ExecutablePath + "\"";
                    if (existing == null)
                    {
                        key.SetValue(name, exe);
                        Msg("已开启开机自启\n路径: " + exe, "开机自启",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        key.DeleteValue(name, false);
                        Msg("已关闭开机自启", "开机自启",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Msg("设置开机自启失败: " + ex.Message, "开机自启",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- Logs ----------
        static void OpenLogs()
        {
            try
            {
                // open the tray log with the default text editor (falls back to notepad)
                string target = _logPath;
                if (!File.Exists(target))
                {
                    File.WriteAllText(target, "(no tray log yet)" + Environment.NewLine);
                }
                Process.Start(new ProcessStartInfo("notepad.exe", "\"" + target + "\"") { UseShellExecute = true });
            }
            catch (Exception ex) { Log("open logs: " + ex.Message); }
        }

        // ---------- Status ----------
        static async void UpdateStatusAsync()
        {
            // Compute on background, marshal the Text update back to the UI thread.
            bool running = await Task.Run(() => IsServiceRunning());
            string v = await Task.Run(() => GetLocalVersion());
            if (_statusItem != null)
                _statusItem.Text = running
                    ? "服务: 运行中 (v" + v + ")"
                    : "服务: 已停止 (v" + v + ")";
        }

        static void ExitApp()
        {
            Log("exit requested");
            StopService();
            Thread.Sleep(800);
            _tray.Visible = false;
            _tray.Dispose();
            Application.Exit();
        }
    }

    // ---------- SVG path parser (M/L/C/Z, supports relative & implicit repeats) ----------
    static class SvgPath
    {
        public static GraphicsPath Parse(string d, int size)
        {
            var gp = new GraphicsPath();
            float cx = 0, cy = 0, sx = 0, sy = 0;
            int i = 0;
            char cmd = '\0';
            bool started = false;

            while (i < d.Length)
            {
                // read next command letter
                while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
                if (i >= d.Length) break;
                if (char.IsLetter(d[i]))
                {
                    cmd = d[i]; i++;
                }
                // else: implicit repeat of last command
                if (cmd == '\0') break;

                bool rel = char.IsLower(cmd);
                char c = char.ToUpperInvariant(cmd);

                if (c == 'Z')
                {
                    gp.CloseFigure();
                    cx = sx; cy = sy;
                    continue;
                }

                if (c == 'M' || c == 'L')
                {
                    float[] p = ReadPair(d, ref i);
                    float x = rel ? cx + p[0] : p[0];
                    float y = rel ? cy + p[1] : p[1];
                    if (c == 'M')
                    {
                        // M = move to: start a new figure at the point, no line yet
                        gp.StartFigure();
                        started = true;
                        sx = x; sy = y;
                        cx = x; cy = y;
                        cmd = 'L'; // implicit lineto after M
                    }
                    else // L
                    {
                        if (!started) { gp.StartFigure(); started = true; }
                        gp.AddLine(cx, cy, x, y);
                        cx = x; cy = y;
                    }
                }
                else if (c == 'C')
                {
                    float[] p = ReadSix(d, ref i);
                    float x1 = rel ? cx + p[0] : p[0], y1 = rel ? cy + p[1] : p[1];
                    float x2 = rel ? cx + p[2] : p[2], y2 = rel ? cy + p[3] : p[3];
                    float x = rel ? cx + p[4] : p[4], y = rel ? cy + p[5] : p[5];
                    gp.AddBezier(cx, cy, x1, y1, x2, y2, x, y);
                    cx = x; cy = y;
                }
                else
                {
                    // unsupported command - skip its params to avoid infinite loop
                    break;
                }
            }

            // normalize to the target size: scale to fit and CENTER the path
            var bounds = gp.GetBounds();
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                float scale = size / Math.Max(bounds.Width, bounds.Height);
                var m = new Matrix();
                m.Scale(scale, scale);
                // center horizontally and vertically, not just top-left align
                float dx = (size - bounds.Width * scale) / 2f - bounds.X * scale;
                float dy = (size - bounds.Height * scale) / 2f - bounds.Y * scale;
                m.Translate(dx, dy);
                gp.Transform(m);
            }
            return gp;
        }

        static float ParseNum(string s)
        {
            if (s.StartsWith(".")) s = "0" + s;
            if (s.EndsWith(".")) s = s + "0";
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        static float[] ReadPair(string d, ref int i)
        {
            float[] r = new float[2];
            for (int k = 0; k < 2; k++) r[k] = ReadNum(d, ref i);
            return r;
        }
        static float[] ReadSix(string d, ref int i)
        {
            float[] r = new float[6];
            for (int k = 0; k < 6; k++) r[k] = ReadNum(d, ref i);
            return r;
        }
        static float ReadNum(string d, ref int i)
        {
            while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
            int start = i;
            while (i < d.Length && !char.IsWhiteSpace(d[i]) && d[i] != ',' && !char.IsLetter(d[i])) i++;
            return ParseNum(d.Substring(start, i - start));
        }
    }

    // ---------- Progress dialog for npm install ----------
    class ProgressForm : Form
    {
        ProgressBar _bar;
        Label _label;
        TextBox _output;
        Process _proc;
        System.Windows.Forms.Timer _timer;
        DateTime _deadline;
        readonly StringBuilder _pending = new StringBuilder();
        bool _completed;

        public bool TimedOut { get; private set; }
        public bool Canceled { get; private set; }

        public ProgressForm(Process proc, TimeSpan timeout)
        {
            _proc = proc;
            _deadline = DateTime.UtcNow.Add(timeout);
            Text = "正在更新 DSH...";
            Width = 460; Height = 320;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false; MinimizeBox = false;

            _label = new Label { Text = "正在执行 npm install -g @deepseek-ai/dsh ...", Left = 12, Top = 12, Width = 420 };
            _bar = new ProgressBar { Style = ProgressBarStyle.Marquee, Left = 12, Top = 40, Width = 420, Height = 20 };
            _output = new TextBox { Left = 12, Top = 70, Width = 420, Height = 180, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9) };
            Controls.Add(_label); Controls.Add(_bar); Controls.Add(_output);

            _proc.OutputDataReceived += AppendProcessOutput;
            _proc.ErrorDataReceived += AppendProcessOutput;
            _proc.BeginOutputReadLine();
            _proc.BeginErrorReadLine();

            _timer = new System.Windows.Forms.Timer { Interval = 300 };
            _timer.Tick += (s, e) =>
            {
                lock (_pending)
                {
                    if (_pending.Length > 0)
                    {
                        _output.AppendText(_pending.ToString());
                        _pending.Clear();
                    }
                }
                if (!_proc.HasExited && DateTime.UtcNow >= _deadline)
                {
                    TimedOut = true;
                    try { _proc.Kill(true); } catch { }
                    _label.Text = "更新超时";
                    _timer.Stop();
                    Close();
                    return;
                }
                if (_proc.HasExited)
                {
                    _completed = true;
                    _timer.Stop();
                    _label.Text = _proc.ExitCode == 0 ? "更新完成" : "更新失败";
                    _bar.Style = ProgressBarStyle.Continuous;
                    _bar.Value = 100;
                    Close();
                }
            };
            Shown += (s, e) => _timer.Start();
            FormClosing += (s, e) =>
            {
                _timer.Stop();
                if (!_completed && !TimedOut && !_proc.HasExited)
                {
                    Canceled = true;
                    try { _proc.Kill(true); } catch { }
                }
            };
        }

        void AppendProcessOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            lock (_pending) _pending.AppendLine(e.Data);
        }
    }
}
