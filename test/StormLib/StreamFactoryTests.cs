using System;
using System.Collections.Generic;
using NUnit.Framework;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormTests.StormLib
{
    [TestFixture]
    public class StreamFactoryTests
    {
        private const string validUri = "google.com";
        private const string invalidUri = "";
        private const string twitchAccount = "twitch.tv/twitch";
        private const string mixlrAccount = "mixlr.com/jeff-gerstmann";
        private const string chaturbateAccount = "chaturbate.com/asianqueen93";
        private const string youtubeAccount = "youtube.com/linustechtips";
        private const string commentChar = "#";

        [Test]
        public void TryCreate_ValidUri_ShouldReturnTrue()
        {
            bool actual = StreamFactory.TryCreate(validUri, out IStream _);

            Assert.True(actual);
        }

        [Test]
        public void TryCreate_InvalidUri_ShouldReturnFalse()
        {
            bool actual = StreamFactory.TryCreate(invalidUri, out IStream _);

            Assert.False(actual);
        }

        [Test]
        public void TryCreate_ValidUri_StreamShouldNotBeNull()
        {
            bool _ = StreamFactory.TryCreate(validUri, out IStream stream);

            Assert.NotNull(stream);
        }

        [Test]
        public void TryCreate_InvalidUri_StreamShouldBeNull()
        {
            bool _ = StreamFactory.TryCreate(invalidUri, out IStream stream);

            Assert.Null(stream);
        }

        [Test]
        public void TryCreate_ValidAccountName_ShouldReturnTrue()
        {
            bool b = StreamFactory.TryCreate(twitchAccount, out IStream _);

            Assert.True(b);
        }

        [Test]
        public void TryCreate_ValidAccountName_ShouldBeNotNull()
        {
            bool _ = StreamFactory.TryCreate(twitchAccount, out IStream stream);

            Assert.NotNull(stream);
        }

        [TestCase(twitchAccount, typeof(TwitchStream))]
        [TestCase(mixlrAccount, typeof(MixlrStream))]
        [TestCase(chaturbateAccount, typeof(ChaturbateStream))]
        [TestCase(youtubeAccount, typeof(YouTubeStream))]
        public void TryCreate_ValidAccountName_StreamShouldBeCorrectType(string account, Type type)
        {
            bool _ = StreamFactory.TryCreate(account, out IStream stream);

            Assert.IsInstanceOf(type, stream);
        }

        [Test]
        public void TryCreate_UriWithoutScheme_ShouldSetToHttps()
        {
            bool _ = StreamFactory.TryCreate(twitchAccount, out IStream stream);

            bool beginsWithHttps = stream.Link.AbsoluteUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            Assert.True(beginsWithHttps);
        }

        [Test]
        public void TryCreate_UriWithHttp_ShouldSetToHttps()
        {
            bool _ = StreamFactory.TryCreate($"http://{twitchAccount}", out IStream stream);

            bool beginsWithHttps = stream.Link.AbsoluteUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            Assert.True(beginsWithHttps);
        }

        [Test]
        public void CreateMany_NoLines_ShouldReturnEmptyResult()
        {
            string[] lines = Array.Empty<string>();

            IReadOnlyCollection<IStream> results = StreamFactory.CreateMany(lines, commentChar);

            Assert.Zero(results.Count);
        }

        [Test]
        public void CreateMany_OnlyCommentedLines_ShouldReturnEmptyResult()
        {
            string[] lines = { commentChar + "twitch.tv/twitch", commentChar + "twitch.tv/xbox", commentChar + "twitch.tv/scarra" };

            IReadOnlyCollection<IStream> results = StreamFactory.CreateMany(lines, commentChar);

            Assert.Zero(results.Count);
        }

        [Test]
        public void CreateMany_ThreeValidAccounts_ShouldReturnThreeStreams()
        {
            string[] lines = { "twitch.tv/twitch", "twitch.tv/xbox", "twitch.tv/scarra" };

            int expected = lines.Length;

            int actual = StreamFactory.CreateMany(lines, commentChar).Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CreateMany_TwoValidAccountsOneCommentedAccount_ShouldReturnTwoStreams()
        {
            string[] lines = { commentChar + "twitch.tv/twitch", "twitch.tv/xbox", "twitch.tv/scarra" };

            int expected = lines.Length - 1;

            int actual = StreamFactory.CreateMany(lines, commentChar).Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CreateMany_ThreeAccountsOneAppearsTwice_ShouldNotReturnSameAccountMoreThanOnce()
        {
            string[] lines = { twitchAccount, twitchAccount, "twitch.tv/scarra" };

            int expected = lines.Length - 1;

            int actual = StreamFactory.CreateMany(lines, commentChar).Count;

            Assert.AreEqual(expected, actual);
        }
    }
}
