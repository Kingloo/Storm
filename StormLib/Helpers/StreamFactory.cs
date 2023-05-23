using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using StormLib.Interfaces;
using StormLib.Services;
using StormLib.Services.Chaturbate;
using StormLib.Services.Kick;
using StormLib.Services.Mixlr;
using StormLib.Services.Rumble;
using StormLib.Services.Twitch;
using StormLib.Services.YouTube;

namespace StormLib.Helpers
{
	public static class StreamFactory
	{
		private const string http = "http://";
		private const string https = "https://";
		private const char defaultCommentChar = '#';

		private const StringComparison sc = StringComparison.CurrentCultureIgnoreCase;

		public static bool TryCreate(string line, [NotNullWhen(true)] out IStream? stream)
		{
			if (String.IsNullOrWhiteSpace(line))
			{
				stream = null;
				return false;
			}

			if (!line.StartsWith(https, sc) && !line.StartsWith(http, sc))
			{
				line = $"{https}{line}";
			}

			if (line.StartsWith(http, sc))
			{
				line = line.Insert(4, "s");
			}

			if (!Uri.TryCreate(line, UriKind.Absolute, out Uri? uri))
			{
				stream = null;
				return false;
			}

			stream = uri.DnsSafeHost.ToLower(CultureInfo.CurrentCulture) switch
			{
				"chaturbate.com" or "www.chaturbate.com" => new ChaturbateStream(uri),
				"kick.com" or "www.kick.com" => new KickStream(uri),
				"mixlr.com" or "www.mixlr.com" => new MixlrStream(uri),
				"rumble.com" or "www.rumble.com" => new RumbleStream(uri),
				"twitch.tv" or "www.twitch.tv" => new TwitchStream(uri),
				"youtube.com" or "www.youtube.com" => new YouTubeStream(uri),
				_ => new UnsupportedStream(uri)
			};

			return true;
		}

		public static IReadOnlyList<IStream> CreateMany(IReadOnlyList<string> lines)
			=> CreateMany(lines, defaultCommentChar);

		public static IReadOnlyList<IStream> CreateMany(IReadOnlyList<string> lines, char commentCharacter)
		{
			ArgumentNullException.ThrowIfNull(lines);

			if (!lines.Any())
			{
				return Array.Empty<IStream>();
			}

			IList<IStream> streams = new List<IStream>();

			foreach (string line in lines.Where(line => !line.StartsWith(commentCharacter)))
			{
				if (TryCreate(line, out IStream? stream))
				{
					streams.Add(stream);
				}
			}

			return streams.Distinct().ToList().AsReadOnly();
		}
	}
}
