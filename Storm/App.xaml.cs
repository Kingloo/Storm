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
            await sm.UpdateAllAsync();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string logFilePath = string.Format("C:\\Users\\{0}\\Documents\\logfile.txt", Environment.UserName);

            using (StreamWriter sw = new StreamWriter(logFilePath))
            {
                sw.WriteLine(string.Format("\n{0}: {1}: {2}: {3}\n", DateTime.Now, Application.Current.ToString(), e.Exception.ToString(), e.Exception.Message.ToString()));
            }
        }
    }
}
