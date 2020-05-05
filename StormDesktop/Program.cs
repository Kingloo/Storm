using System;
using System.Globalization;
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
            app.InitializeComponent();

            int exitCode = app.Run();

            if (exitCode != 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "exited with code {0}", exitCode);

                LogStatic.Message(message);
            }

            return exitCode;
        }
    }
}
