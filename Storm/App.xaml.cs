using System;
using System.IO;
using System.Windows;

namespace Storm
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            StreamManager sm = (StreamManager)Application.Current.Resources["streamManager"];

            await sm.LoadUrlsFromFileAsync();
        }
        
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Misc.LogException(e.Exception);
        }
    }
}
