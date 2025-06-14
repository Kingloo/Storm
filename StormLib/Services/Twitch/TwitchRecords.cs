using System;

namespace StormLib.Services.Twitch
{
	public record TwitchDisplayName(string DisplayName);
	public record TwitchGame(Int64 Id, string Name);
}
