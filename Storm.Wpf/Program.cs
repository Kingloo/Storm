using System;
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
                string message = $"Storm exited with code {exitCode}";

                Log.LogMessage(message);
            }

            return exitCode;
        }
    }
}
