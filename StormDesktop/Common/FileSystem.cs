using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StormDesktop.Common
{
	public static class FileSystem
	{
		private const char defaultCommentChar = '#';
		private static readonly Encoding defaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

		public static void EnsureDirectoryExists(string? folder)
		{
			ArgumentNullException.ThrowIfNull(folder);

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
		}

		public static void EnsureFileExists(string path)
		{
			if (!File.Exists(path))
			{
				EnsureDirectoryExists(Path.GetDirectoryName(path));

				using (File.Create(path)) { }
			}
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<IReadOnlyList<string>> LoadLinesFromFileAsync(string path)
			=> LoadLinesFromFileAsync(path, defaultCommentChar, defaultEncoding, CancellationToken.None);

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<IReadOnlyList<string>> LoadLinesFromFileAsync(string path, char comment)
			=> LoadLinesFromFileAsync(path, comment, defaultEncoding, CancellationToken.None);

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<IReadOnlyList<string>> LoadLinesFromFileAsync(string path, Encoding encoding)
			=> LoadLinesFromFileAsync(path, defaultCommentChar, encoding, CancellationToken.None);

		public static async ValueTask<IReadOnlyList<string>> LoadLinesFromFileAsync(string path, char commentChar, Encoding encoding, CancellationToken cancellationToken)
		{
			if (char.IsWhiteSpace(commentChar))
			{
				throw new ArgumentException("comment char cannot be whitespace", nameof(commentChar));
			}

			List<string> lines = new List<string>();

			FileStream fsAsync = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

			try
			{
				using StreamReader sr = new StreamReader(fsAsync, encoding);

				string? line = string.Empty;

				while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
				{
					cancellationToken.ThrowIfCancellationRequested();

					bool shouldAddLine = line.Length switch
					{
						0 => true,
						_ => line[0] != commentChar
					};

					if (shouldAddLine)
					{
						lines.Add(line);
					}
				}
			}
			finally
			{
				await fsAsync.DisposeAsync().ConfigureAwait(false);
			}

			return lines.AsReadOnly();
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask WriteLinesToFileAsync(string[] lines, string path, FileMode mode)
			=> WriteLinesToFileAsync(lines, path, mode, defaultEncoding, CancellationToken.None);

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask WriteLinesToFileAsync(string[] lines, string path, FileMode mode, Encoding encoding)
			=> WriteLinesToFileAsync(lines, path, mode, encoding, CancellationToken.None);

		public static async ValueTask WriteLinesToFileAsync(string[] lines, string path, FileMode mode, Encoding encoding, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(lines);

			FileStream fsAsync = new FileStream(path, mode, FileAccess.Write, FileShare.None, bufferSize: 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

			try
			{
				using StreamWriter sw = new StreamWriter(fsAsync, encoding);

				foreach (string line in lines)
				{
					await sw.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
				}

				await sw.FlushAsync().ConfigureAwait(false);
			}
			finally
			{
				await fsAsync.DisposeAsync().ConfigureAwait(false);
			}
		}
	}
}
