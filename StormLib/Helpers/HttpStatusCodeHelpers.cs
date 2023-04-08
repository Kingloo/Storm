using System.Net;

namespace StormLib.Helpers
{
	public static class HttpStatusCodeHelpers
	{
		public static string FormatStatusCode(HttpStatusCode statusCode)
		{
			return $"{(int)statusCode} ({statusCode})";
		}
	}
}
