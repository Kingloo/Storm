using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StormLib.Common
{
	public enum Severity
	{
		None = 0,
		Debug = 1,
		Information = 2,
		Warning = 3,
		Error = 4
	}

	public interface ILog
	{
		string Path { get; }
		Severity Severity { get; }

		void Message(string message, Severity severity);
		Task MessageAsync(string message, Severity severity);

		void Exception(Exception ex);
		void Exception(Exception ex, string message);
		void Exception(Exception ex, bool includeStackTrace);
		void Exception(Exception ex, string message, bool includeStackTrace);

		Task ExceptionAsync(Exception ex);
		Task ExceptionAsync(Exception ex, string message);
		Task ExceptionAsync(Exception ex, bool includeStackTrace);
		Task ExceptionAsync(Exception ex, string message, bool includeStackTrace);
	}

	public class Log : ILog
	{
		public string Path { get; } = string.Empty;
		public Severity Severity { get; } = Severity.None;

		public Log(string path, Severity severity)
		{
			Path = path;
			Severity = severity;
		}

		public void Message(string message, Severity severity)
		{
			if (severity >= Severity)
			{
				string text = FormatMessage(message);

				WriteToFile(text, Path);
			}
		}

		public Task MessageAsync(string message, Severity severity)
		{
			if (severity >= Severity)
			{
				string text = FormatMessage(message);

				return WriteToFileAsync(text, Path);
			}
			else
			{
				return Task.CompletedTask;
			}
		}


		public void Exception(Exception ex)
			=> Exception(ex, string.Empty, false);

		public void Exception(Exception ex, string message)
			=> Exception(ex, message, false);

		public void Exception(Exception ex, bool includeStackTrace)
			=> Exception(ex, string.Empty, includeStackTrace);

		public void Exception(Exception ex, string message, bool includeStackTrace)
		{
			if (ex is null) { throw new ArgumentNullException(nameof(ex)); }

			string text = FormatException(ex, message, includeStackTrace);

			Message(text, Severity.Error);
		}

		public Task ExceptionAsync(Exception ex)
			=> ExceptionAsync(ex, string.Empty, false);

		public Task ExceptionAsync(Exception ex, string message)
			=> ExceptionAsync(ex, message, false);

		public Task ExceptionAsync(Exception ex, bool includeStackTrace)
			=> ExceptionAsync(ex, string.Empty, includeStackTrace);

		public Task ExceptionAsync(Exception ex, string message, bool includeStackTrace)
		{
			if (ex is null) { throw new ArgumentNullException(nameof(ex)); }

			string text = FormatException(ex, message, includeStackTrace);

			return MessageAsync(text, Severity.Error);
		}


		private static string FormatMessage(string message)
		{
			var cc = CultureInfo.CurrentCulture;

			string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss (zzz)", cc);
			string processName = Process.GetCurrentProcess().MainModule?.ModuleName ?? "unknown process";

			return string.Format(cc, "{0} - {1} - {2}", timestamp, processName, message);
		}

		private static string FormatException(Exception ex, string message, bool includeStackTrace)
		{
			var sb = new StringBuilder();

			sb.Append(ex.GetType().FullName);
			sb.Append(" - ");
			sb.Append(ex.Message);

			if (!String.IsNullOrWhiteSpace(message))
			{
				sb.Append(" - ");
				sb.Append(message);
			}

			if (includeStackTrace)
			{
				sb.AppendLine(ex.StackTrace);
			}

			return sb.ToString();
		}


		private static void WriteToFile(string text, string path)
		{
			FileStream? fs = default;

			try
			{
				fs = new FileStream(
					path,
					FileMode.Append,
					FileAccess.Write,
					FileShare.None,
					4096,
					FileOptions.None);

				using (StreamWriter sw = new StreamWriter(fs))
				{
					fs = null;

					sw.WriteLine(text);

					sw.Flush();
				}
			}
			catch (IOException) { }
			finally
			{
				fs?.Close();
			}
		}

		private static async Task WriteToFileAsync(string text, string path)
		{
			FileStream? fsAsync = default;

			try
			{
				fsAsync = new FileStream(
					path,
					FileMode.Append,
					FileAccess.Write,
					FileShare.None,
					4096,
					FileOptions.Asynchronous);

				using (StreamWriter sw = new StreamWriter(fsAsync))
				{
					fsAsync = null;

					await sw.WriteLineAsync(text).ConfigureAwait(false);

					await sw.FlushAsync().ConfigureAwait(false);
				}
			}
			catch (IOException) { }
			finally
			{
				fsAsync?.Close();
			}
		}
	}

	public class NullLog : ILog
	{
		public string Path => string.Empty;

		public Severity Severity => Severity.None;

		public void Message(string message, Severity severity) { }
		public Task MessageAsync(string message, Severity severity) => Task.CompletedTask;

		public void Exception(Exception ex) { }
		public void Exception(Exception ex, string message) { }
		public void Exception(Exception ex, bool includeStackTrace) { }
		public void Exception(Exception ex, string message, bool includeStackTrace) { }

		public Task ExceptionAsync(Exception ex) => Task.CompletedTask;
		public Task ExceptionAsync(Exception ex, string message) => Task.CompletedTask;
		public Task ExceptionAsync(Exception ex, bool includeStackTrace) => Task.CompletedTask;
		public Task ExceptionAsync(Exception ex, string message, bool includeStackTrace) => Task.CompletedTask;
	}

	public static class LogStatic
	{
		private const string defaultFileName = "logfile.txt";
		private static readonly string defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		private static readonly string defaultPath = Path.Combine(defaultDirectory, defaultFileName);

		private static readonly Log instance = new Log(defaultPath, Severity.Information);

		public static void Message(string message) => instance.Message(message, Severity.Error);
		public static Task MessageAsync(string message) => instance.MessageAsync(message, Severity.Error);

		public static void Exception(Exception ex) => instance.Exception(ex);
		public static void Exception(Exception ex, string message) => instance.Exception(ex, message);
		public static void Exception(Exception ex, bool includeStackTrace) => instance.Exception(ex, includeStackTrace);
		public static void Exception(Exception ex, string message, bool includeStackTrace) => instance.Exception(ex, message, includeStackTrace);

		public static Task ExceptionAsync(Exception ex) => instance.ExceptionAsync(ex);
		public static Task ExceptionAsync(Exception ex, string message) => instance.ExceptionAsync(ex, message);
		public static Task ExceptionAsync(Exception ex, bool includeStackTrace) => instance.ExceptionAsync(ex, includeStackTrace);
		public static Task ExceptionAsync(Exception ex, string message, bool includeStackTrace) => instance.ExceptionAsync(ex, message, includeStackTrace);
	}
}
