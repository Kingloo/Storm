using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StormLib.Services.Twitch
{
	internal sealed class TwitchGameComparer : IEqualityComparer<TwitchGame>
	{
		public bool Equals(TwitchGame? x, TwitchGame? y)
		{
			return (x, y) switch
			{
				(TwitchGame, TwitchGame) => x.Id.Equals(y.Id),
				(null, null) => true,
				_ => false
			};
		}

		public int GetHashCode([DisallowNull] TwitchGame obj)
		{
			return obj.Id.GetHashCode();
		}
	}
}