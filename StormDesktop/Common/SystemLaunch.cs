using System;
using System.Diagnostics;
using System.IO;

namespace StormDesktop.Common
{
	public static class SystemLaunch
	{
		public static bool Path(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			return File.Exists(path) && Launch(path);
		}

		public static bool Uri(Uri uri)
		{
			if (uri is null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			return uri.IsAbsoluteUri && Launch(uri.AbsoluteUri);
		}

		public static bool Launch(string launchString)
		{
			if (String.IsNullOrWhiteSpace(launchString))
			{
				throw new ArgumentNullException(nameof(launchString));
			}

			ProcessStartInfo pInfo = new ProcessStartInfo(launchString)
			{
				UseShellExecute = true
			};

			return LaunchInternal(pInfo);
		}

		public static bool Launch(ProcessStartInfo pInfo)
		{
			if (pInfo is null)
			{
				throw new ArgumentNullException(nameof(pInfo));
			}

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
