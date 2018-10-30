using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Storm.Wpf.Common
{
    public class FileLoader
    {
        public FileInfo File { get; } = null;

        public FileLoader(FileInfo file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
        }


        public string Load() => Load(Encoding.UTF8);

        public string Load(Encoding encoding) => System.IO.File.ReadAllText(File.FullName, encoding);


        public string[] LoadLines() => LoadLines(Encoding.UTF8);

        public string[] LoadLines(Encoding encoding) => System.IO.File.ReadAllLines(File.FullName, encoding);


        public Task<string> LoadAsync() => LoadAsync(Encoding.UTF8);

        public async Task<string> LoadAsync(Encoding encoding)
        {
            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(
                    File.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                using (StreamReader sr = new StreamReader(fsAsync, encoding))
                {
                    fsAsync = null;

                    return await sr.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                return string.Empty;
            }
            finally
            {
                fsAsync?.Close();
            }
        }


        public Task<string[]> LoadLinesAsync() => LoadLinesAsync(Encoding.UTF8);

        public async Task<string[]> LoadLinesAsync(Encoding encoding)
        {
            List<string> lines = new List<string>();

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(
                    File.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                using (StreamReader sr = new StreamReader(fsAsync, encoding))
                {
                    fsAsync = null;

                    string line = string.Empty;

                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        lines.Add(line);
                    }
                }
            }
            catch (IOException)
            {
                return Array.Empty<string>();
            }
            finally
            {
                fsAsync?.Close();
            }

            return lines.ToArray();
        }
    }
}
