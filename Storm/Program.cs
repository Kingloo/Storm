using System;
using System.Globalization;
using System.IO;
using Storm.Common;
using Storm.DataAccess;

namespace Storm
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            FileInfo urlsFile = GetUrlFilePath();

            if (urlsFile == null)
            {
                Log.LogMessage($"StormUrls.txt not found");

                return -1;
            }
            
            TxtRepo urlsRepo = new TxtRepo(urlsFile);

            App app = new App(urlsRepo);

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "exited with code {0}", exitCode);

                Log.LogMessage(errorMessage);
            }

            return exitCode;
        }

        private static FileInfo GetUrlFilePath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!Directory.Exists(dir))
            {
                return null;
            }

#if DEBUG
            string filename = "StormUrls-test.txt";
#else
            string filename = "StormUrls.txt";
#endif

            string fullPath = Path.Combine(dir, filename);

            return File.Exists(fullPath) ? new FileInfo(fullPath) : null;
        }
    }
}
