using System;

namespace StormLib.Services.Mixlr
{
	public class MixlrOptions
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public Uri? ApiUri { get; init; }

		public MixlrOptions() { }
	}
}