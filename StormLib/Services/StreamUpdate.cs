using System;
using StormLib.Interfaces;

namespace StormLib.Services
{
	public class StreamUpdate<TStream> where TStream : IStream
	{
		public TStream Stream { get; init; }
		public Action<TStream> Action { get; init; }

		public StreamUpdate(TStream stream, Action<TStream> action)
		{
			ArgumentNullException.ThrowIfNull(stream);
			ArgumentNullException.ThrowIfNull(action);

			Stream = stream;
			Action = action;
		}
	}
}
