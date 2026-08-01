using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SmartSystemMenu.Forms;
using SmartSystemMenu.Settings;
using SmartSystemMenu.Utils;

namespace SmartSystemMenu
{
    static class Program
    {
        private static Mutex _mutex;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            Application.ThreadException += OnThreadException;

            // 单实例
            _mutex = new Mutex(true, "SmartSystemMenuSelectorMutex", out bool createdNew);
            if (!createdNew)
            {
                return;
            }

            var assemblyDir = AssemblyUtils.AssemblyDirectory;
            var settingsFileName = Path.Combine(assemblyDir, "SmartSystemMenu.xml");
            var languageFileName = Path.Combine(assemblyDir, "Language.xml");
            var windowFileName = Path.Combine(assemblyDir, "Window64.xml");

            var settings = File.Exists(settingsFileName) && File.Exists(languageFileName)
                ? ApplicationSettingsFile.Read(settingsFileName, languageFileName)
                : new ApplicationSettings();
            var windowSettings = File.Exists(windowFileName) ? WindowSettings.Read(windowFileName) : new WindowSettings();

            // 高 DPI 支持
            if (settings.EnableHighDPI)
            {
                SystemUtils.EnableHighDPISupport();
            }

            // 命令行：仅支持退出实例
            if (args.Length > 0 && args[0].ToLower() == "/quit")
            {
                return;
            }

            Application.Run(new MainForm(settings, windowSettings, IntPtr.Zero));
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                MessageBox.Show(ex.ToString(), "SmartSystemMenu Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "SmartSystemMenu Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
