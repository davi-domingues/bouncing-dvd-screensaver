// Bouncing DVD Screensaver - fullscreen, dimmed, muted looping YouTube video that keeps
// Windows awake so long-running work (e.g. AI coding agents) is never interrupted.
//
// - Lowers the built-in panel brightness to the lowest non-zero level (never off).
// - Mouse movement/click temporarily restores brightness without closing.
// - ESC closes the window, restores brightness and ends the process.
//
// Built with the C# 5 compiler that ships with .NET Framework 4.x (see build.ps1).
using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

[assembly: System.Reflection.AssemblyTitle("Bouncing DVD Screensaver")]
[assembly: System.Reflection.AssemblyProduct("Bouncing DVD Screensaver")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace BouncingDvdScreensaver
{
    sealed class Options
    {
        public const string DefaultVideoId = "5mGuCdlCcNM"; // Bouncing DVD Logo Screensaver 4K 60fps
        public const int DefaultWakeSeconds = 5;

        public string VideoId = DefaultVideoId;
        public int WakeSeconds = DefaultWakeSeconds;

        public const string Usage =
            "Usage: bouncing-dvd-screensaver [--video <id|url>] [--wake-seconds <n>]\n\n" +
            "  --video <id|url>     YouTube video ID or URL to play (default: " + DefaultVideoId + ")\n" +
            "  --wake-seconds <n>   Seconds of full brightness after mouse activity, 1-3600 (default: 5)\n" +
            "  --help               Show this help\n\n" +
            "Controls: move/click the mouse to brighten temporarily, ESC to exit.";

        static readonly Regex IdPattern = new Regex("^[A-Za-z0-9_-]{11}$");
        static readonly Regex UrlPattern = new Regex(@"(?:youtube(?:-nocookie)?\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/|live/)|youtu\.be/)([A-Za-z0-9_-]{11})");

        // Returns null on success, or an error message.
        public static string TryParse(string[] args, Options o, out bool help)
        {
            help = false;
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                string value = null;
                int eq = a.IndexOf('=');
                if (a.StartsWith("--") && eq > 0) { value = a.Substring(eq + 1); a = a.Substring(0, eq); }

                switch (a.ToLowerInvariant())
                {
                    case "--help": case "-h": case "/?":
                        help = true;
                        return null;
                    case "--video":
                        if (value == null) { if (++i >= args.Length) return "--video requires a value."; value = args[i]; }
                        string id = ParseVideoId(value);
                        if (id == null) return "Invalid YouTube video ID or URL: " + value;
                        o.VideoId = id;
                        break;
                    case "--wake-seconds":
                        if (value == null) { if (++i >= args.Length) return "--wake-seconds requires a value."; value = args[i]; }
                        int n;
                        if (!int.TryParse(value, out n) || n < 1 || n > 3600) return "--wake-seconds must be an integer between 1 and 3600.";
                        o.WakeSeconds = n;
                        break;
                    default:
                        return "Unknown option: " + args[i];
                }
            }
            return null;
        }

        public static string ParseVideoId(string s)
        {
            s = s.Trim();
            if (IdPattern.IsMatch(s)) return s;
            var m = UrlPattern.Match(s);
            return m.Success ? m.Groups[1].Value : null;
        }
    }

    // Brightness of the built-in display through WMI (WmiMonitorBrightness).
    static class Brightness
    {
        static int original = -1;
        static int minimum = 1;
        public static bool Supported;

        public static void Initialize()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness"))
                {
                    foreach (ManagementObject o in s.Get())
                    {
                        original = Convert.ToInt32(o["CurrentBrightness"]);
                        // Lowest level above 0, so the backlight is never turned off.
                        var levels = o["Level"] as byte[];
                        minimum = 100;
                        if (levels != null)
                            foreach (var n in levels)
                                if (n > 0 && n < minimum) minimum = n;
                        if (minimum == 100) minimum = 1;
                        Supported = true;
                        break;
                    }
                }
            }
            catch { Supported = false; }
        }

        static void Set(int value)
        {
            if (!Supported) return;
            try
            {
                using (var s = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightnessMethods"))
                {
                    foreach (ManagementObject o in s.Get())
                        o.InvokeMethod("WmiSetBrightness", new object[] { (uint)1, (byte)value });
                }
            }
            catch { }
        }

        public static void Dim() { Set(minimum); }
        public static void Restore() { if (original >= 0) Set(original); }
    }

    sealed class ScreensaverForm : Form
    {
        const int WM_HOTKEY = 0x0312, HOTKEY_ESC = 1;
        const uint ES_CONTINUOUS = 0x80000000, ES_SYSTEM_REQUIRED = 0x1, ES_DISPLAY_REQUIRED = 0x2;

        [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint vk);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("kernel32.dll")] static extern uint SetThreadExecutionState(uint flags);

        readonly Options options;
        readonly WebView2 web = new WebView2();
        readonly Timer dimTimer = new Timer();
        bool awake;
        bool shutDown;

        public ScreensaverForm(Options options)
        {
            this.options = options;
            Text = "Bouncing DVD Screensaver";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen.Bounds;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = System.Drawing.Color.Black;
            KeyPreview = true;

            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = System.Drawing.Color.Black;
            Controls.Add(web);

            dimTimer.Interval = options.WakeSeconds * 1000;
            dimTimer.Tick += delegate { dimTimer.Stop(); Sleep(); };

            Load += OnLoad;
            FormClosing += delegate { ShutDown(); };
        }

        async void OnLoad(object sender, EventArgs e)
        {
            // Keep the display on and the system awake while running.
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
            // Global ESC hotkey: works even if the web view has keyboard focus.
            RegisterHotKey(Handle, HOTKEY_ESC, 0, (uint)Keys.Escape);
            Sleep();

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BouncingDvdScreensaver", "WebView2");
                var envOptions = new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required");
                var env = await CoreWebView2Environment.CreateAsync(null, dataDir, envOptions);
                await web.EnsureCoreWebView2Async(env);
            }
            catch (Exception ex)
            {
                ShutDown();
                MessageBox.Show("Could not start the WebView2 runtime.\n\n" + ex.Message +
                    "\n\nInstall it from https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                    "Bouncing DVD Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            var core = web.CoreWebView2;
            core.IsMuted = true; // no audio, regardless of the page
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            // Serve the page from a real https origin: the YouTube player requires a valid referrer.
            core.SetVirtualHostNameToFolderMapping("bouncing-dvd-screensaver.local",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web"), CoreWebView2HostResourceAccessKind.Allow);
            core.WebMessageReceived += OnWebMessage;
            core.Navigate("https://bouncing-dvd-screensaver.local/index.html?v=" + Uri.EscapeDataString(options.VideoId));
            web.Focus();
        }

        void OnWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var msg = e.TryGetWebMessageAsString();
            if (msg == "esc") Close();
            else if (msg == "activity") Wake();
        }

        void Wake()
        {
            if (!awake)
            {
                awake = true;
                if (Brightness.Supported) Brightness.Restore();
                else SetSoftwareDim(false);
            }
            dimTimer.Stop();
            dimTimer.Start();
        }

        void Sleep()
        {
            awake = false;
            if (Brightness.Supported) Brightness.Dim();
            else SetSoftwareDim(true);
        }

        // Fallback for displays without WMI brightness control (e.g. external monitors).
        void SetSoftwareDim(bool on)
        {
            if (web.CoreWebView2 != null)
                web.CoreWebView2.ExecuteScriptAsync("document.body.classList.toggle('dim'," + (on ? "true" : "false") + ")");
        }

        void ShutDown()
        {
            if (shutDown) return;
            shutDown = true;
            dimTimer.Stop();
            UnregisterHotKey(Handle, HOTKEY_ESC);
            SetThreadExecutionState(ES_CONTINUOUS);
            Brightness.Restore();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { Close(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ESC) { Close(); return; }
            base.WndProc(ref m);
        }
    }

    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var options = new Options();
            bool help;
            string error = Options.TryParse(args, options, out help);
            if (error != null || help)
            {
                MessageBox.Show((error != null ? error + "\n\n" : "") + Options.Usage, "Bouncing DVD Screensaver",
                    MessageBoxButtons.OK, error != null ? MessageBoxIcon.Error : MessageBoxIcon.Information);
                return error != null ? 2 : 0;
            }

            bool firstInstance;
            using (new System.Threading.Mutex(true, "BouncingDvdScreensaver_SingleInstance", out firstInstance))
            {
                if (!firstInstance) return 0; // already running

                Brightness.Initialize();
                // Restore brightness even on unexpected failures.
                AppDomain.CurrentDomain.UnhandledException += delegate { Brightness.Restore(); };
                AppDomain.CurrentDomain.ProcessExit += delegate { Brightness.Restore(); };
                Application.ThreadException += delegate { Brightness.Restore(); Application.Exit(); };

                Application.EnableVisualStyles();
                Application.Run(new ScreensaverForm(options));
            }
            return 0;
        }
    }
}
