using System;

namespace StormLib.Services.Twitch
{
	public record class TwitchDisplayName(string DisplayName);
	public record class TwitchGame(Int64 Id, string Name);
}
