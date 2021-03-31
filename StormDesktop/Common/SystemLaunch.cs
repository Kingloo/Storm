using System;
using System.Diagnostics;
using System.IO;

namespace StormDesktop.Common
{
    public static class SystemLaunch
    {
        public static bool Path(string path)
        {
            return File.Exists(path) && Launch(path);
        }

        public static bool Uri(Uri uri)
        {
            return uri.IsAbsoluteUri && Launch(uri.AbsoluteUri);
        }

        public static bool Launch(ProcessStartInfo pInfo)
        {
            return LaunchInternal(pInfo);
        }

        public static bool Launch(string launchString)
        {
            ProcessStartInfo pInfo = new ProcessStartInfo(launchString)
            {
                UseShellExecute = true
            };

            return LaunchInternal(pInfo);
        }

        private static bool LaunchInternal(ProcessStartInfo pInfo)
        {
            using Process p = new Process
            {
                StartInfo = pInfo
            };

            return p.Start();
        }
    }
}
