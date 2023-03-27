namespace StormLib.Services.Twitch
{
	public record class TwitchDisplayName(string DisplayName);
	public record class TwitchGameId(int Id);
	public record class TwitchGameName(string Name);
	public record class TwitchTopicId(int Id);
	public record class TwitchTopicName(string Name);
	public record class TwitchGame(TwitchGameId Id, TwitchGameName Name);
}