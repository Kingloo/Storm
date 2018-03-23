using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Storm.DataAccess
{
    public static class FileSystem
    {
        public static Task<string[]> GetLinesAsync(FileInfo file) => GetLinesAsync(file, false);

        public static Task<string[]> GetLinesAsync(FileInfo file, bool createIfAbsent)
        {
            if (file == null) { throw new ArgumentNullException(nameof(file)); }

            FileMode mode = FileMode.Open;

            if (!file.Exists)
            {
                if (createIfAbsent)
                {
                    mode = FileMode.OpenOrCreate;
                }
                else
                {
                    throw new FileNotFoundException(nameof(file));
                }
            }

            return GetLinesAsyncImpl(file, mode);
        }

        private static async Task<string[]> GetLinesAsyncImpl(FileInfo file, FileMode mode)
        {
            var lines = new List<string>();

            var fsAsync = new FileStream(
                file.FullName,
                mode,
                FileAccess.Read,
                FileShare.None,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            using (StreamReader sr = new StreamReader(fsAsync))
            {
                fsAsync = null;

                string line = string.Empty;

                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    lines.Add(line);
                }
            }

            fsAsync?.Dispose();

            return lines.ToArray();
        }
    }
}
