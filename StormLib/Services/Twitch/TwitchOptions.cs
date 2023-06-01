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
		public IList<Int64> UnwantedGameIds { get; init; } = new List<Int64>();

		public TwitchOptions() { }
	}
}
