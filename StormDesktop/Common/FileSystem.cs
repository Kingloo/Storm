using System;
using System.IO;
using System.Threading.Tasks;

namespace StormDesktop.Common
{
    public static class FileSystem
    {
        public static async Task<string[]> ReadAllLinesAsync(string filePath)
        {
            try
            {
                return await File.ReadAllLinesAsync(filePath).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                return Array.Empty<string>();
            }
        }
    }
}
