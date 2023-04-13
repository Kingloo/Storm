using System;
using System.Collections.Generic;
using StormLib.Interfaces;

namespace StormLib.Services.Twitch
{
	public class TwitchOptions : IUpdateIntervalOption
	{
		public int UpdateIntervalSeconds { get; init; } = Constants.DefaultUpdateIntervalSeconds;
		public Uri? GraphQlApiUri { get; init; }
		public IList<Header> Headers { get; init; } = new List<Header>();
		public int MaxStreamsPerUpdate { get; init; } = 0;
		public IList<int> UnwantedGameIds { get; init; } = new List<int>();
#pragma warning disable CA1056
		public string EmbeddedPlayerUriFormat { get; init; } = string.Empty;
#pragma warning restore CA1056

		public TwitchOptions() { }
	}
}
