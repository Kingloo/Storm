using System;

namespace StormLib.Services.Chaturbate
{
	public class ChaturbateOptions
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromMinutes(3d);

		public ChaturbateOptions() { }
	}
}