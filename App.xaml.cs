using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Aspan_Ground
{
    public partial class App : Application
    {
        public App()
        {
            // UI thread yakalanmayan hataları
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            // Non-UI thread hataları
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string msg = e.Exception?.ToString() ?? "Bilinmeyen hata";
            File.WriteAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                "[UI-Thread]\n" + msg);
            MessageBox.Show("Uygulama hatası:\n\n" + e.Exception?.Message, "Kritik Hata");
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string msg = e.ExceptionObject?.ToString() ?? "Bilinmeyen hata";
            File.WriteAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
                "[BG-Thread]\n" + msg);
        }
    }
}
