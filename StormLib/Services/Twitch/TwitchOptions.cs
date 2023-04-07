using System;
using System.Collections.Generic;
using StormLib.Interfaces;

namespace StormLib.Services.Twitch
{
	public class TwitchOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(1.5d);
		public Uri? GraphQlApiUri { get; init; }
		public IList<Header> Headers { get; init; } = new List<Header>();
		public int MaxStreamsPerUpdate { get; init; } = 0;
		public IReadOnlyList<TwitchGameId> UnwantedGameIds { get; init; } = new List<TwitchGameId>();
		public IReadOnlyList<TwitchTopicId> UnwantedTopicIds { get; init; } = new List<TwitchTopicId>();
#pragma warning disable CA1056
		public string EmbeddedPlayerUriFormat { get; init; } = string.Empty;
#pragma warning restore CA1056

		public TwitchOptions() { }
	}
}