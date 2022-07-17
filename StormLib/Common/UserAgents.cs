using System;
using System.Collections.Generic;
using System.Linq;

namespace StormLib.Common
{
    public static class UserAgents
	{
		public const string HeaderName = "User-Agent";

#pragma warning disable CA1707
		public const string Firefox_102_Windows = nameof(Firefox_102_Windows);
		public const string Firefox_91_ESR_Linux = nameof(Firefox_91_ESR_Linux);
		public const string Edge_103_Windows = nameof(Edge_103_Windows);
		public const string Edge_103_Linux = nameof(Edge_103_Linux);
#pragma warning restore CA1707

		private static readonly IDictionary<string, string> agents = new Dictionary<string, string>
		{
			{ Firefox_102_Windows, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:102.0) Gecko/20100101 Firefox/102.0" },
			{ Firefox_91_ESR_Linux, "Mozilla/5.0 (X11; Linux x86_64; rv:91.0) Gecko/20100101 Firefox/91.0" },
			{ Edge_103_Windows, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37" },
			{ Edge_103_Linux, "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37" }
		};

		public static string Get(string browser)
		{
			return agents[browser];
		}

		public static string GetRandomUserAgent()
		{
#pragma warning disable CA5394
			Random random = new Random();

			int randomNumber = random.Next(0, agents.Count - 1);

			return agents.Values.ToArray()[randomNumber];
#pragma warning restore CA5394
		}
	}
}
