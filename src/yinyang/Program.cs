using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using PeanutButter.TrayIcon;
using PeanutButter.Utils;
using Timer = System.Windows.Forms.Timer;

namespace yinyang
{
    static class Program
    {
        private static TrayIcon _icon;
        private static Icon _lightIcon;
        private static Icon _darkIcon;
        private static bool _switching;
        private static Timer _timer;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var img = LoadBitmapResourceByName("icon.png");
            var ico = Icon.FromHandle(img.GetHicon());
            _lightIcon = ico;
            
            var darkBmp = LoadBitmapResourceByName("icon.png");
            darkBmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _darkIcon = Icon.FromHandle(darkBmp.GetHicon());
            
            _icon = new TrayIcon(
                IsDarkTheme()
                    ? _darkIcon
                    : _lightIcon
            );
            
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += CheckIcon;


            _switching = false;
            GenerateMenu();
            _icon.AddMouseClickHandler(MouseClicks.Single, MouseButtons.Left, SwitchThemes);
            _icon.NotifyIcon.MouseMove += UpdateTooltip;
            
            
            _icon.Show();
            _timer.Tick += CheckIcon;
            _timer.Interval = 5000;
            _timer.Start();

            Application.Run();
        }

        private static void UpdateTooltip(object sender, MouseEventArgs e)
        {
            UpdateToolTipDebounced();
        }

        private static DateTime _lastTooltipUpdate = DateTime.MinValue;
        private static void UpdateToolTipDebounced()
        {
            if (DateTime.Now - _lastTooltipUpdate < TimeSpan.FromMilliseconds(1000))
            {
                return;
            }
            _lastTooltipUpdate = DateTime.Now;
            
            var menuName = IsDarkTheme()
                ? LightMenuItem
                : DarkMenuItem;
            _icon.NotifyIcon.Text = menuName;
        }

        private static void GenerateMenu()
        {
            AddFlipMenuItem();
            _icon.AddMenuSeparator();
            _icon.RemoveMenuItem("E&xit");
            _icon.AddMenuItem("E&xit", () =>
            {
                _timer.Stop();
                _icon.Hide();
                Application.Exit();
            });
        }

        private static Bitmap LoadBitmapResourceByName(string name)
        {
            var asm = typeof(Program).Assembly;
            var resourceName = asm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(name));
            using var resourceStream = asm.GetManifestResourceStream(resourceName);
            var img = Image.FromStream(resourceStream) as Bitmap;
            return img;
        }

        private static Icon _scheduledChange;
        
        private static void CheckIcon(object sender, EventArgs e)
        {
            if (_scheduledChange is not null)
            {
                _icon.Icon = _scheduledChange;
                _scheduledChange = null;
                return;
            }

            var expected = IsDarkTheme()
                ? _darkIcon
                : _lightIcon;


            if (expected != _icon.Icon)
            {
                _scheduledChange = expected;
            }
        }


        private const string DarkMenuItem = "Descend into the dark...";
        private const string DarkSwitchMessage = "Descending into the dark...";
        private const string LightMenuItem = "Ascend into the light...";
        private const string LightSwitchMessage = "Ascending into the light...";

        private static void AddFlipMenuItem()
        {
            var isDarkTheme = IsDarkTheme();
            var menuName = isDarkTheme
                ? LightMenuItem
                : DarkMenuItem;
            var oldMenuName = isDarkTheme
                ? DarkMenuItem
                : LightMenuItem;
            _icon.RemoveMenuItem(oldMenuName);
            _icon.AddMenuItem(menuName, SwitchThemes);
        }

        private static void SwitchThemes()
        {
            if (_switching)
            {
                _icon.ShowBalloonTipFor(5000, "Patience, friend.", "Still busy switching...", ToolTipIcon.Warning);
                return;
            }
            _switching = true;

            if (IsDarkTheme())
            {
                SwitchToLightTheme();
            }
            else
            {
                SwitchToDarkTheme();
            }
        }

        private static void SwitchToDarkTheme()
        {
            ShowMessage(DarkSwitchMessage);
            _icon.Icon = _darkIcon;
            SwitchToTheme(0);
        }

        private static void SwitchToLightTheme()
        {
            ShowMessage(LightSwitchMessage);
            _icon.Icon = _lightIcon;
            SwitchToTheme(1);
        }

        private static void ShowMessage(string message)
        {
            _icon.ShowBalloonTipFor(5000, message, "This may take a little while. Please be patient.", ToolTipIcon.None);
        }

        private static void SwitchToTheme(int keyValue)
        {
            Task.Run(() =>
            {
                // delay just a little so the notification can show
                Thread.Sleep(500);
                using var key = OpenPersonalizeKey();
                key.SetValue(AppsKey, keyValue, RegistryValueKind.DWord);
                key.SetValue(SystemKey, keyValue, RegistryValueKind.DWord);
                
                GenerateMenu();
                
                _switching = false;
            });
        }

        private const string AppsKey = "AppsUseLightTheme";
        private const string SystemKey = "SystemUsesLightTheme";

        private static bool IsDarkTheme()
        {
            using var key = OpenPersonalizeKey();
            return key.GetValue(AppsKey).ToString().AsInteger() == 0;
        }

        private static RegistryKey OpenPersonalizeKey() => Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true
        );
    }
}