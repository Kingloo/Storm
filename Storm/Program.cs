using System;
using System.Globalization;
using Storm.DataAccess;

[assembly: CLSCompliant(true)]
namespace Storm
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            string filePath = string.Format(CultureInfo.InvariantCulture, @"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
            TxtRepo urlsRepo = new TxtRepo(filePath);

            App app = new App(urlsRepo);
            app.InitializeComponent();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "exited with code {0}", exitCode);

                Utils.LogMessage(errorMessage);
            }

            return exitCode;
        }
    }
}
