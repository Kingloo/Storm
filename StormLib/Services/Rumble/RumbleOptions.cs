using System;
using StormLib.Interfaces;

namespace StormLib.Services.Rumble
{
	public class RumbleOptions : IUpdateIntervalOption
	{
		public int UpdateIntervalSeconds { get; init; } = Constants.DefaultUpdateIntervalSeconds;
		public string LiveMarker { get; init; } = string.Empty;

		public RumbleOptions() { }
	}
}
