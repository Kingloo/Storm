using System;
using StormLib.Interfaces;

namespace StormLib.Services.Rumble
{
	public class RumbleOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.Zero;
		public string LiveMarker { get; init; } = string.Empty;

		public RumbleOptions() { }
	}
}
