using StormLib.Interfaces;

namespace StormLib.Services.Chaturbate
{
	public class ChaturbateOptions : IUpdateIntervalOption
	{
		public int UpdateIntervalSeconds { get; init; } = Constants.DefaultUpdateIntervalSeconds;

		public ChaturbateOptions() { }
	}
}
