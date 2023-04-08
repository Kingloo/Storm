using System;
using System.Collections.Generic;
using StormLib.Interfaces;

namespace StormLib.Services.Kick
{
	public class KickOptions : IUpdateIntervalOption
	{
		public int UpdateIntervalSeconds { get; init; } = Constants.DefaultUpdateIntervalSeconds;
		public Uri? ApiUri { get; init; }
		public IList<Header> Headers { get; init; } = new List<Header>();

		public KickOptions() { }
	}
}
