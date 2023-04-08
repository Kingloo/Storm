using System;
using StormLib.Interfaces;

namespace StormLib.Services.Rumble
{
	public class RumbleOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public string LiveMarker { get; init; } = string.Empty;

		public RumbleOptions() { }
	}
}
