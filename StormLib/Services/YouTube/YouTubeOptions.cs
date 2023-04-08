using System;
using StormLib.Interfaces;

namespace StormLib.Services.YouTube
{
	public class YouTubeOptions : IUpdateIntervalOption
	{
		public int UpdateIntervalSeconds { get; init; } = Constants.DefaultUpdateIntervalSeconds;
		public string LiveMarker { get; init; } = string.Empty;

		public YouTubeOptions() { }
	}
}
