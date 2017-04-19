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

            if (String.IsNullOrWhiteSpace(fullPath))
            {
                Log.LogMessage($"StormUrls.txt not found");

                return -1;
            }

            TxtRepo urlsRepo = new TxtRepo(fullPath);

            App app = new App(urlsRepo);

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "exited with code {0}", exitCode);

                Log.LogMessage(errorMessage);
            }

            return exitCode;
        }

        private static string GetUrlFilePath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

#if DEBUG
            string filename = "StormUrls-test.txt";
#else
            string filename = "StormUrls.txt";
#endif

            return Path.Combine(dir, filename);
        }
    }
}
