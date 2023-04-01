using System.Collections.Generic;

namespace StormLib
{
	public class StormOptions
	{
		public string StreamsFilePath { get; init; } = string.Empty;
		
		public IList<Header> CommonHeaders { get; init; } = new List<Header>();

		public StormOptions() { }
	}
}