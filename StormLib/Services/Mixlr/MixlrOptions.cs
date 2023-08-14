using System;
using StormLib.Interfaces;

namespace StormLib.Services.Mixlr
{
	public class MixlrOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.Zero;
		public Uri? ApiUri { get; init; }

		public MixlrOptions() { }
	}
}
