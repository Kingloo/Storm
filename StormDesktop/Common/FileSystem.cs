using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StormDesktop.Common
{
    public static class FileSystem
    {
        public static void EnsureDirectoryExists(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);

                if (!Directory.Exists(folder))
                {
                    throw new DirectoryNotFoundException($"{folder} could not be created");
                }
            }
        }

        public static void EnsureFileExists(string path)
        {
            if (!File.Exists(path))
            {
				EnsureDirectoryExists(new FileInfo(path).DirectoryName);

				using (File.Create(path)) { }

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"file could not be created ({path})", path);
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<string[]> LoadLinesFromFileAsync(string path)
            => LoadLinesFromFileAsync(path, string.Empty, Encoding.UTF8);

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<string[]> LoadLinesFromFileAsync(string path, string comment)
            => LoadLinesFromFileAsync(path, comment, Encoding.UTF8);
        
        public static async ValueTask<string[]> LoadLinesFromFileAsync(string path, string comment, Encoding encoding)
        {
            List<string> lines = new List<string>();

            FileStream fsAsync = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            
            try
            {
                using (StreamReader sr = new StreamReader(fsAsync, encoding))
                {
                    string? line = string.Empty;

                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        bool shouldAddLine = true;

                        if (!String.IsNullOrWhiteSpace(comment))
                        {
                            if (line.StartsWith(comment, StringComparison.OrdinalIgnoreCase))
                            {
                                shouldAddLine = false;
                            }
                        }

                        if (shouldAddLine)
                        {
                            lines.Add(line);
                        }
                    }
                }
            }
            finally
            {
                if (!(fsAsync is null))
                {
                    await fsAsync.DisposeAsync().ConfigureAwait(false);
                }
            }
            
            return lines.ToArray();
        }

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<bool> WriteLinesToFileAsync(string[] lines, string path, FileMode mode)
            => WriteLinesToFileAsync(lines, path, mode, Encoding.UTF8);

        public static async ValueTask<bool> WriteLinesToFileAsync(string[] lines, string path, FileMode mode, Encoding encoding)
        {
            FileStream fsAsync = new FileStream(path, mode, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            
            try
            {
                using (StreamWriter sw = new StreamWriter(fsAsync, encoding))
                {
                    foreach (string line in lines)
                    {
                        await sw.WriteLineAsync(line).ConfigureAwait(false);
                    }

                    await sw.FlushAsync().ConfigureAwait(false);
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            finally
            {
                if (!(fsAsync is null))
                {
                    await fsAsync.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
    }
}