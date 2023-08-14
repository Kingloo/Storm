using System;
using StormLib.Interfaces;

namespace StormLib.Services.YouTube
{
	public class YouTubeOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.Zero;
		public string LiveMarker { get; init; } = string.Empty;

		public YouTubeOptions() { }
	}
}
