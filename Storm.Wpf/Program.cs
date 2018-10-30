using System;
using System.IO;
using Storm.Wpf.Common;
using Storm.Wpf.GUI;

namespace Storm.Wpf
{
    public static class Program
    {
#if DEBUG
        private const string fileName = "StormUrls-test.txt";
#else
        private const string fileName = "StormUrls.txt";
#endif

        private static string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string streamsFilePath = Path.Combine(directory, fileName);

        [STAThread]
        public static int Main(string[] args)
        {
            FileInfo streamsFile = new FileInfo(streamsFilePath);

            FileLoader fileLoader = new FileLoader(streamsFile);

            App storm = new App(fileLoader);
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
