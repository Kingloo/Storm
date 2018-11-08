using System;
using System.Windows;
using System.Windows.Threading;
using Storm.Wpf.Common;

namespace Storm.Wpf.GUI
{
    public partial class App : Application
    {
        public App(FileLoader fileLoader)
        {
            InitializeComponent();

            MainWindow = new MainWindow(fileLoader);

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
