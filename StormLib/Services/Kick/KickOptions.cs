using System;

namespace StormLib.Services.Kick
{
	public class KickOptions
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public Uri ApiUri { get; init; }

		public KickOptions() { }
	}
}