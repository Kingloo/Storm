using System;
using System.Globalization;
using System.IO;
using Storm.Common;

namespace Storm
{
    public static class Program
    {
        private static string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

#if DEBUG
        private static string filename = "StormUrls-test.txt";
#else
        private static string filename = "StormUrls.txt";
#endif

        [STAThread]
        public static int Main()
        {
            string fullPath = Path.Combine(directory, filename);
            FileInfo file = new FileInfo(fullPath);

            App app = new App(file);

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "exited with code {0}", exitCode);

                Log.LogMessage(errorMessage);
            }

            return exitCode;
        }
    }
}
