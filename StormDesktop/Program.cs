using System;
using StormDesktop.Common;
using StormDesktop.Gui;

namespace StormDesktop
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            App app = new App();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string message = "";

                LogStatic.Message(message, Severity.Error);
            }

            return exitCode;
        }
    }
}
