using System;
using System.Globalization;
using System.IO;
using Storm.Common;
using Storm.DataAccess;

namespace Storm
{
    public static class Program
    {
        public enum ReturnCode
        {
            Success = 0,
            Failure = -1,
            DirectoryDoesNotExist = -2,
            CannotCreateFileInDirectory = -3
        }
        
        [STAThread]
        public static int Main()
        {
            ReturnCode result = GetUrlsFile(out FileInfo file);

            if (result != ReturnCode.Success)
            {
                return (int)result;
            }
            
            TxtRepo urlsRepo = new TxtRepo(file);

            App app = new App(urlsRepo);

            int exitCode = app.Run();

            if (exitCode != (int)ReturnCode.Success)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "exited with code {0}", exitCode);

                Log.LogMessage(errorMessage);
            }

            return exitCode;
        }

        private static ReturnCode GetUrlsFile(out FileInfo file)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (String.IsNullOrWhiteSpace(directory))
            {
                file = null;

                return ReturnCode.DirectoryDoesNotExist;
            }

#if DEBUG
            string filename = "StormUrls-test.txt";
#else
            string filename = "StormUrls.txt";
#endif

            string fullPath = Path.Combine(directory, filename);

            if (File.Exists(fullPath))
            {
                file = new FileInfo(fullPath);

                return ReturnCode.Success;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    file = new FileInfo(fullPath);

                    return ReturnCode.Success;
                }
            }
            catch
            {
                file = null;

                return ReturnCode.Failure;
            }
        }
    }
}
