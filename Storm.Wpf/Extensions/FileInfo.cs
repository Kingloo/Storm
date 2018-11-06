using System;
using System.Diagnostics;
using System.IO;

namespace Storm.Wpf.Extensions
{
    public static class FileInfoExtensions
    {
        public static void Launch(this FileInfo file)
            => Launch(file, new ProcessStartInfo(file.FullName));

        public static void Launch(this FileInfo file, ProcessStartInfo pInfo)
        {
            if (file is null) { throw new ArgumentNullException(nameof(file)); }

            if (file.Exists)
            {
                Process.Start(pInfo);
            }
        }
    }
}
