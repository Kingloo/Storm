using System;
using System.Diagnostics;
using System.IO;

namespace Storm.Wpf.Extensions
{
    public static class FileInfoExtensions
    {
        public static void Launch(this FileSystemInfo file)
        {
            if (file is null) { throw new ArgumentNullException(nameof(file)); }

            Launch(file, new ProcessStartInfo(file.FullName));
        }

        public static void Launch(this FileSystemInfo file, ProcessStartInfo processStartInfo)
        {
            if (file is null) { throw new ArgumentNullException(nameof(file)); }

            if (file.Exists)
            {
                Process.Start(processStartInfo);
            }
        }
    }
}
