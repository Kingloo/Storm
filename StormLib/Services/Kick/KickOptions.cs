using System;
using System.Collections.Generic;
using StormLib.Interfaces;

namespace StormLib.Services.Kick
{
	public class KickOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(3d);
		public Uri? ApiUri { get; init; }
		public IList<Header> Headers { get; init; } = new List<Header>();

		public KickOptions() { }
	}
}