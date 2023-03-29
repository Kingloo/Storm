using System;
using System.Collections.Generic;

namespace StormLib.Services.Kick
{
	public class KickOptions
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(2d);
		public Uri ApiUri { get; init; }
		public IDictionary<HeaderName, HeaderValue> Headers { get; init; } = new Dictionary<HeaderName, HeaderValue>();

		public KickOptions() { }
	}
}