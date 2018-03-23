using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Storm.Common;
using Storm.ViewModels;
using Storm.Views;

namespace Storm
{
    public partial class App : Application
    {
        public App(FileInfo file)
        {
            InitializeComponent();
            
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(file)
            };

            MainWindow.Show();

            // without this it will reuse the same connection for a given service
            // e.g. checking 10 twitch accounts will use the same connection for all 10 tries
            // in SEQUENCE, bad
            // increase this to allow them to occur in parallel
            // improves performance significantly
            ServicePointManager.DefaultConnectionLimit = 10;
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.LogException(e.Exception, true);
        }
    }
}
