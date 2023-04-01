using System;
using NUnit.Framework;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Services;

namespace StormTests.StormLib.Streams
{
	[TestFixture]
	public class UnsupportedStreamTests
	{
		[Test]
		public void Ctor_NullUri_ShouldThrowNullArgEx()
		{
#pragma warning disable CS8625
			Assert.Throws<ArgumentNullException>(() => new UnsupportedStream(null));
#pragma warning restore CS8625
		}

		[Test]
		public void Ctor_UriAndNameShouldBeSame()
		{
			string unsupportedUri = "https://google.com";

			var u = new UnsupportedStream(new Uri(unsupportedUri));

			Assert.AreEqual(u.Link.AbsoluteUri, u.Name);
		}

		[Test]
		public void StreamFactoryTryCreateAndNewUp_ShouldReturnEqualsObjects()
		{
			string account = "https://google.com";

			UnsupportedStream newUp = new UnsupportedStream(new Uri(account, UriKind.Absolute));
			bool b = StreamFactory.TryCreate(account, out IStream? stream);

			if (!b)
			{
				throw new Exception("TryCreate returned false, when it really shouldn't have");
			}

			if (stream is null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			UnsupportedStream tryCreate = (UnsupportedStream)stream;

			bool actual = newUp.Equals(tryCreate);
			bool actual2 = newUp == tryCreate;

			Assert.IsTrue(actual);
			Assert.IsTrue(actual2);
		}
	}
}
