using System;
using Storm.DataAccess;

namespace Storm
{
    public class Program
    {
        [STAThread]
        public static int Main()
        {
            TxtRepo urlsRepo = new TxtRepo(Globals.UrlsFilePath);

            App app = new App(urlsRepo);

            app.InitializeComponent();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string errorMessage = string.Format("exited with code {0}", exitCode);

                Utils.LogMessage(errorMessage);
            }

            return exitCode;
        }
    }
}
