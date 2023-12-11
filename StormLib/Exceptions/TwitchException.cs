using System;

namespace StormLib.Exceptions
{
	public class TwitchException : Exception
	{
		public TwitchException() { }

		public TwitchException(string? message)
			: base(message)
		{ }

		public TwitchException(string? message, Exception? innerException)
			: base(message, innerException)
		{ }
	}
}
