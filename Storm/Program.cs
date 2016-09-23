using System;
using System.Globalization;
using System.IO;
using Storm.DataAccess;

namespace Storm
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            string fullPath = GetUrlFilePath();
            TxtRepo urlsRepo = new TxtRepo(fullPath);

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

        private static string GetUrlFilePath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filename = "StormUrls.txt";

            return Path.Combine(dir, filename);
        }
    }
}
