using System;
using Storm.Wpf.GUI;

namespace Storm.Wpf
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            App storm = new App();

            storm.InitializeComponent();

            int exitCode = storm.Run();

            if (exitCode != 0)
            {
                // log
            }

            return exitCode;
        }
    }
}
