using System.Collections.Generic;

namespace StormLib
{
	public class StormOptions
	{
		public string StreamsFilePath { get; init; } = string.Empty;
		public IDictionary<HeaderName, HeaderValue> CommonHeaders { get; init; } = new Dictionary<HeaderName, HeaderValue>();

		public StormOptions() { }
	}
}