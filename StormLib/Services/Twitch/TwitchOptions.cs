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
		public IReadOnlyList<TwitchGameId> UnwantedGameIds { get; init; } = new List<TwitchGameId>();
		public IReadOnlyList<TwitchTopicId> UnwantedTopicIds { get; init; } = new List<TwitchTopicId>();
		public Uri EmbeddedPlayerUriFormat { get; init; }

		public TwitchOptions() { }
	}
}