using System;
using System.Net;
using StormLib.Interfaces;

namespace StormLib
{
	public class Result<TStream> where TStream : IStream
	{
		public TStream Stream { get; init; }
		public Action<TStream> Action { get; init; } = (_) => { };
		public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.Unused;
		public string Message { get; init; } = string.Empty;

		public Result(TStream stream, HttpStatusCode statusCode)
		{
			ArgumentNullException.ThrowIfNull(stream);

			Stream = stream;
			StatusCode = statusCode;
		}
	}
}
