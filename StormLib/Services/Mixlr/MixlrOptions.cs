using System;
using StormLib.Interfaces;

namespace StormLib.Services.Mixlr
{
	public class MixlrOptions : IUpdateIntervalOption
	{
		public int UpdateIntervalSeconds { get; init; } = Constants.DefaultUpdateIntervalSeconds;
		public Uri? ApiUri { get; init; }

		public MixlrOptions() { }
	}
}
