using System;
using StormLib.Interfaces;

namespace StormLib.Services.YouTube
{
	public class YouTubeOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public string LiveMarker { get; init; } = string.Empty;

		public YouTubeOptions() { }
	}
}