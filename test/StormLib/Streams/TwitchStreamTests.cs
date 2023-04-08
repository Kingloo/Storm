using System;
using NUnit.Framework;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Services.Twitch;

namespace StormTests.StormLib.Streams
{
	[TestFixture]
	public class TwitchStreamTests
	{
		private const string validTwitchAccount = "https://twitch.tv/twitch";

		[Test]
		public void Ctor_LinkHasScheme_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => new TwitchStream(new Uri(validTwitchAccount)));
		}

		[Test]
		public void Ctor_LinkDoesNotHaveScheme_ThrowsUriFormatException()
		{
			Assert.Throws<UriFormatException>(() => new TwitchStream(new Uri("twitch.tv/twitch")));
		}

		[Test]
		public void Equals_WhenSameAccount_ReturnsTrue()
		{
			TwitchStream a = new TwitchStream(new Uri(validTwitchAccount));
			TwitchStream b = new TwitchStream(new Uri(validTwitchAccount));

			bool actual = a.Equals(b);
			bool actual2 = a == b;

			Assert.IsTrue(actual);
			Assert.IsTrue(actual2);
		}

		[Test]
		public void Equals_StreamFactoryTryCreateAndNewUp_ReturnEqualsObjects()
		{
			TwitchStream a = new TwitchStream(new Uri(validTwitchAccount));
			bool _ = StreamFactory.TryCreate(validTwitchAccount, out IStream? stream);

			if (stream is null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			TwitchStream b = (TwitchStream)stream;

			bool actual = a.Equals(b);
			bool actual2 = a == b;

			Assert.IsTrue(actual);
			Assert.IsTrue(actual2);
		}
	}
}
