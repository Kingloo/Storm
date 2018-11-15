using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Storm.Wpf.Common;

namespace Storm.Wpf.GUI
{
    public partial class App : Application
    {
#if DEBUG
        private const string fileName = "StormUrls-test.txt";
#else
        private const string fileName = "StormUrls.txt";
#endif

        public App()
        {
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string streamsFilePath = Path.Combine(directory, fileName);

            FileLoader loader = new FileLoader(new FileInfo(streamsFilePath));

            MainWindow = new MainWindow(loader);

            MainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                Log.LogException(ex);

                e.Handled = true;
            }
            else
            {
                string message = "an empty DispatcherUnhandledException was thrown";

                Log.LogMessage(message);

                e.Handled = false;
            }
        }
    }
}
