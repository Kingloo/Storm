using System;
using StormLib.Interfaces;

namespace StormLib.Services.Chaturbate
{
	public class ChaturbateOptions : IUpdateIntervalOption
	{
		public TimeSpan UpdateInterval { get; init; } = TimeSpan.Zero;

		public ChaturbateOptions() { }
	}
}
