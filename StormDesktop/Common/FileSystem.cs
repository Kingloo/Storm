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
			if (String.IsNullOrWhiteSpace(folder))
			{
				throw new ArgumentNullException(nameof(folder));
			}

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);

				if (!Directory.Exists(folder))
				{
					throw new DirectoryNotFoundException($"{folder} could not be created");
				}
			}
		}

		public static void EnsureFileExists(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			if (!File.Exists(path))
			{
				EnsureDirectoryExists(new FileInfo(path).DirectoryName);

				using (File.Create(path)) { }

				if (!File.Exists(path))
				{
					throw new FileNotFoundException($"file could not be created ({path})", path);
				}
			}
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<string[]> LoadLinesFromFileAsync(string path)
			=> LoadLinesFromFileAsync(path, defaultCommentChar, defaultEncoding, CancellationToken.None);

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask<string[]> LoadLinesFromFileAsync(string path, CancellationToken cancellationToken)
			=> LoadLinesFromFileAsync(path, defaultCommentChar, defaultEncoding, cancellationToken);

		public static async ValueTask<string[]> LoadLinesFromFileAsync(string path, char comment, Encoding encoding, CancellationToken token)
		{
			List<string> lines = new List<string>();

			FileStream fsAsync = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

			try
			{
				using StreamReader sr = new StreamReader(fsAsync, encoding);

				string? line = string.Empty;

				while (!String.IsNullOrEmpty(line = await sr.ReadLineAsync(token).ConfigureAwait(false)))
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					bool shouldAddLine = true;

					if (!Char.IsWhiteSpace(comment))
					{
						if (line[0] == comment)
						{
							shouldAddLine = false;
						}
					}

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

			return lines.ToArray();
		}

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask WriteLinesToFileAsync(string[] lines, string path, FileMode mode)
			=> WriteLinesToFileAsync(lines, path, mode, defaultEncoding, CancellationToken.None);

		[System.Diagnostics.DebuggerStepThrough]
		public static ValueTask WriteLinesToFileAsync(string[] lines, string path, FileMode mode, CancellationToken cancellationToken)
			=> WriteLinesToFileAsync(lines, path, mode, defaultEncoding, cancellationToken);

		public static async ValueTask WriteLinesToFileAsync(string[] lines, string path, FileMode mode, Encoding encoding, CancellationToken token)
		{
			ArgumentNullException.ThrowIfNull(lines, nameof(lines));

			if (lines.Length == 0)
			{
				return;
			}

			FileStream fsAsync = new FileStream(path, mode, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);

			try
			{
				using StreamWriter sw = new StreamWriter(fsAsync, encoding);

				foreach (string line in lines)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					await sw.WriteLineAsync(line).ConfigureAwait(false);
				}

				await sw.FlushAsync(CancellationToken.None).ConfigureAwait(false);
			}
			finally
			{
				await fsAsync.DisposeAsync().ConfigureAwait(false);
			}
		}
	}
}
