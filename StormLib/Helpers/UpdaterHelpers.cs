using System.Collections.Generic;
using System.Net.Http;

namespace StormLib.Helpers
{
	internal static class UpdaterHelpers
	{
		internal static void AddHeaders(IDictionary<HeaderName, HeaderValue> headers, HttpRequestMessage requestMessage)
		{
			foreach (KeyValuePair<HeaderName, HeaderValue> kvp in headers)
			{
				requestMessage.Headers.Add(kvp.Key.Value, kvp.Value.Value);
			}
		}
	}
}