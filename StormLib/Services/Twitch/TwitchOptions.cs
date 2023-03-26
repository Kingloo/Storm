using System;
using System.Collections.Generic;

namespace StormLib.Services.Twitch
{
	public class TwitchOptions
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public Uri GraphQlApiUri { get; init; }
		public IDictionary<HeaderName, HeaderValue> Headers { get; init; } = new Dictionary<HeaderName, HeaderValue>();
		public int MaxStreamsPerUpdate { get; init; } = 0;
		public TwitchGameId[] UnwantedGameIds { get; init; } = Array.Empty<TwitchGameId>();
		public TwitchTopicId[] UnwantedTopicIds { get; init; } = Array.Empty<TwitchTopicId>();
		public Uri EmbeddedPlayerUriFormat { get; init; }

		public TwitchOptions() { }
	}
}