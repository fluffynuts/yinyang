using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using PeanutButter.TrayIcon;
using PeanutButter.Utils;

namespace yinyang
{
    static class Program
    {
        private static TrayIcon _icon;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var asm = typeof(Program).Assembly;
            var resourceName = asm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith("icon.png"));
            using var resourceStream = asm.GetManifestResourceStream(resourceName);
            var img = Image.FromStream(resourceStream) as Bitmap;
            _icon = new TrayIcon(img);

            AddFlipMenuItem();
            _icon.AddMouseClickHandler(MouseClicks.Single, MouseButtons.Left, SwitchThemes);
            _icon.AddMenuItem("E&xit", Application.Exit);

            _icon.Show();

            Application.Run();
        }


        private const string DarkMenuItem = "Descend into the dark...";
        private const string DarkSwitchMessage = "Descending into the dark...";
        private const string LightMenuItem = "Ascend into the light...";
        private const string LightSwitchMessage = "Ascending into the light...";

        private static void AddFlipMenuItem()
        {
            var menuName = IsDarkTheme()
                ? LightMenuItem
                : DarkMenuItem;
            var oldMenuName = IsDarkTheme()
                ? DarkMenuItem
                : LightMenuItem;
            _icon.RemoveMenuItem(oldMenuName);
            _icon.AddMenuItem(menuName, SwitchThemes);
        }

        private static void SwitchThemes()
        {
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
            SwitchToTheme(0);
        }

        private static void SwitchToLightTheme()
        {
            ShowMessage(LightSwitchMessage);
            SwitchToTheme(1);
        }

        private static void ShowMessage(string message)
        {
            _icon.ShowBalloonTipFor(5000, "", message, ToolTipIcon.None);
        }

        private static void SwitchToTheme(int keyValue)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                using var key = OpenPersonalizeKey();
                key.SetValue(AppsKey, keyValue, RegistryValueKind.DWord);
                key.SetValue(SystemKey, keyValue, RegistryValueKind.DWord);
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