using System;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.RuntimeInformation;

namespace StormLib.Common
{
	public static class RuntimeCircumstance
	{
		private const string windowsDirectory = @"C:\Program Files\dotnet";
		private const string linuxDirectory = "/usr/share/dotnet";
		private const string macOSX = "Mac OSX";
		private const string freeBSD = "FreeBSD";
		private const string unknown = "unknown platform";
		private const string dontKnowMessage = "I don't know what the default dotnet install directory is on {0}";

		private static readonly string currentProcessDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty) ?? string.Empty;

		public static string GetRealLocation()
		{
			return IsRunByDotnet() switch
			{
				true => AppContext.BaseDirectory, // `dotnet .\path\to\lib.dll`
				false => currentProcessDirectory // `.\path\to\executable.exe`
			};
		}

		public static bool IsRunByDotnet()
		{
			if (IsOSPlatform(OSPlatform.Windows))
			{
				return currentProcessDirectory.Equals(windowsDirectory, StringComparison.OrdinalIgnoreCase);
			}
			else if (IsOSPlatform(OSPlatform.Linux))
			{
				return currentProcessDirectory.Equals(linuxDirectory, StringComparison.Ordinal);
			}
			else if (IsOSPlatform(OSPlatform.OSX))
			{
				throw new PlatformNotSupportedException(string.Format(CultureInfo.CurrentCulture, dontKnowMessage, macOSX));
			}
			else if (IsOSPlatform(OSPlatform.FreeBSD))
			{
				throw new PlatformNotSupportedException(string.Format(CultureInfo.CurrentCulture, dontKnowMessage, freeBSD));
			}
			else
			{
				throw new PlatformNotSupportedException(unknown);
			}
		}
	}
}
