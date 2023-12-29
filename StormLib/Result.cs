using System;
using System.Net;
using StormLib.Interfaces;

namespace StormLib
{
	public class Result<TStream> where TStream : notnull, IStream
	{
		public DateTimeOffset Finished { get; }
		public TStream Stream { get; init; }
		public Action<TStream> Action { get; init; } = (_) => { };
		public HttpStatusCode? StatusCode { get; init; } = null;
		public string Message { get; init; } = string.Empty;

		public Result(TStream stream)
			: this(stream, (_) => { })
		{ }

		public Result(TStream stream, Action<TStream> action)
		{
			ArgumentNullException.ThrowIfNull(stream);
			ArgumentNullException.ThrowIfNull(action);

			Stream = stream;
			Action = action;

			Finished = DateTimeOffset.UtcNow;
		}
	}
}
