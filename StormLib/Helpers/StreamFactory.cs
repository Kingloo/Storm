using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Helpers
{
	public static class StreamFactory
	{
		private const string http = "http://";
		private const string https = "https://";

		private const StringComparison sc = StringComparison.CurrentCultureIgnoreCase;

		public static bool TryCreate(string line, out IStream? stream)
		{
			if (!line.StartsWith(https, sc) && !line.StartsWith(http, sc))
			{
				line = $"{https}{line}";
			}

			if (line.StartsWith(http, sc))
			{
				line = line.Insert(4, "s");
			}

			if (!Uri.TryCreate(line, UriKind.Absolute, out Uri uri))
			{
				stream = null;
				return false;
			}

			string host = uri.DnsSafeHost;

			try
			{
				if (host.EndsWith("twitch.tv", sc))
				{
					stream = new TwitchStream(uri);
				}
				else if (host.EndsWith("chaturbate.com", sc))
				{
					stream = new ChaturbateStream(uri);
				}
				else if (host.EndsWith("mixer.com", sc))
				{
					stream = new UnsupportedStream(uri);
				}
				else if (host.EndsWith("mixlr.com", sc))
				{
					stream = new MixlrStream(uri);
				}
				else if (host.EndsWith("youtube.com", sc))
				{
					stream = new YouTubeStream(uri);
				}
				else
				{
					stream = new UnsupportedStream(uri);
				}
			}
			catch (ArgumentException)
			{
				stream = null;
				return false;
			}

			return stream is not null;
		}

		public static IReadOnlyCollection<IStream> CreateMany(string[] lines, string commentCharacter)
		{
			ConcurrentBag<IStream> streams = new ConcurrentBag<IStream>();

			if (lines.Length > 0)
			{
				IEnumerable<string> nonCommentLines = lines.Where(l => !l.StartsWith(commentCharacter, StringComparison.OrdinalIgnoreCase));

				Parallel.ForEach(nonCommentLines, (line, loopState) =>
				{
					// Don't do a "if (!streams.Contains)" check before adding
					// Race condition!

					if (TryCreate(line, out IStream? stream)
						&& stream is not null)
					{
						streams.Add(stream);
					}
				});
			}

			return streams.AsEnumerable().Distinct().ToList().AsReadOnly();
		}
	}
}
