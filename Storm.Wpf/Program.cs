using System;
using System.Globalization;
using Storm.Wpf.Common;
using Storm.Wpf.GUI;

namespace Storm.Wpf
{
    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            App storm = new App();

            int exitCode = storm.Run();

            if (exitCode != 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "Storm exited with code {0}", exitCode);

                Log.Message(message);
            }

            return exitCode;
        }
    }
}
