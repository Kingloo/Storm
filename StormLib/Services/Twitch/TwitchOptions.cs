using System;
using System.Collections.Generic;

namespace StormLib.Services.Twitch
{
	public class TwitchOptions
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public Uri? GraphQlApiUri { get; init; }
		public IList<Header> Headers { get; init; } = Array.Empty<Header>();
		public int MaxStreamsPerUpdate { get; init; } = 0;
		public IReadOnlyList<TwitchGameId> UnwantedGameIds { get; init; } = new List<TwitchGameId>();
		public IReadOnlyList<TwitchTopicId> UnwantedTopicIds { get; init; } = new List<TwitchTopicId>();
#pragma warning disable CA1056
		public string EmbeddedPlayerUriFormat { get; init; } = string.Empty;
#pragma warning restore CA1056

		public TwitchOptions() { }
	}
}