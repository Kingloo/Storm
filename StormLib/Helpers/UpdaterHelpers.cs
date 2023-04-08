using System.Collections.Generic;
using System.Net.Http;

namespace StormLib.Helpers
{
	internal static class UpdaterHelpers
	{
		internal static void AddHeaders(IList<Header> headers, HttpRequestMessage requestMessage)
		{
			foreach (Header header in headers)
			{
				requestMessage.Headers.Add(header.Name, header.Value);
			}
		}
	}
}
