using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

namespace Storm
{
    public class Program
    {
        private static string _stormUrlsFilePath = string.Format(@"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
        public static string StormUrlsFilePath { get { return _stormUrlsFilePath; } }

        private static readonly List<string> _urls = new List<string>();
        public static List<string> URLs { get { return _urls; } }

        [STAThread]
        public static int Main(string[] args)
        {
            IEnumerable<string> loaded = LoadUrlsFromFileAsync().Result;

            if (loaded == null)
            {
                MessageBox.Show("There was a fatal problem with your URLs file.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return 0;
            }

            URLs.AddList<string>(loaded);

            App app = new App();
            app.InitializeComponent();

            return app.Run();
        }

        public static async Task<IEnumerable<string>> LoadUrlsFromFileAsync()
        {
            List<string> toReturn = new List<string>();

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(_stormUrlsFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);
            }
            catch (FileNotFoundException)
            {
                if (fsAsync != null)
                {
                    fsAsync.Close();
                }

                File.Create(_stormUrlsFilePath);

                return Enumerable.Empty<string>();
            }

            using (fsAsync)
            {
                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    string line = string.Empty;

                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (line.StartsWith("#") == false)
                        {
                            toReturn.Add(line);
                        }
                    }
                }
            }

            return toReturn;
        }
    }
}
