using System;
using System.Diagnostics;
using System.Globalization;
using StormDesktop.Gui;

namespace StormDesktop
{
	public static class Program
	{
		[STAThread]
		public static int Main()
		{
			App app = new App();

			int exitCode = app.Run();

			if (exitCode != 0)
			{
				string message = string.Format(CultureInfo.CurrentCulture, "exited with code {0}", exitCode);

				Console.Error.WriteLine(message);

				Debug.WriteLineIf(Debugger.IsAttached, message);
			}

			return exitCode;
		}
	}
}
