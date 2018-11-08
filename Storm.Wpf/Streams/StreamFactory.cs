using System;

namespace Storm.Wpf.Streams
{
    public static class StreamFactory
    {
        private const string http = "http://";
        private const string https = "https://";

        private static readonly StringComparison sc = StringComparison.CurrentCultureIgnoreCase;

        /// <summary>
        /// Attempts to create the correct stream object from a line of text.
        /// </summary>
        /// <param name="rawAccountLink">The account link as raw text.</param>
        /// <param name="stream">If succcessful, the IStream object, e.g. TwitchStream or ChaturbateStream.</param>
        /// <returns>Will fail for several reasons, including if the line was a comment, or didn't parse as a Uri.</returns>
        public static bool TryCreate(string rawAccountLink, char comment, out StreamBase stream)
        {
            if (rawAccountLink is null) { throw new ArgumentNullException(nameof(rawAccountLink)); }

            // comment character, each comment must be on its own line
            if (rawAccountLink.StartsWith(comment.ToString(), sc))
            {
                stream = null;
                return false;
            }

            if (!rawAccountLink.StartsWith(https, sc) && !rawAccountLink.StartsWith(http, sc))
            {
                rawAccountLink = $"{https}{rawAccountLink}";
            }

            if (rawAccountLink.StartsWith(http, sc))
            {
                rawAccountLink = rawAccountLink.Insert(4, "s");
            }

            if (!Uri.TryCreate(rawAccountLink, UriKind.Absolute, out Uri uri))
            {
                stream = null;
                return false;
            }

            string host = uri.DnsSafeHost;

            try
            {
                if (host.EndsWith("twitch.tv", sc))
                {
                    stream = new TwitchStream(uri);
                }
                else if (host.EndsWith("chaturbate.com", sc))
                {
                    stream = new ChaturbateStream(uri);
                }
                else if (host.EndsWith("mixer.com", sc))
                {
                    stream = new MixerStream(uri);
                }
                else if (host.EndsWith("mixlr.com", sc))
                {
                    stream = new MixlrStream(uri);
                }
                else if (host.EndsWith("youtube.com", sc))
                {
                    stream = new YouTubeStream(uri);
                }
                else if (host.EndsWith("smashcast.tv", sc))
                {
                    stream = new SmashcastStream(uri);
                }
                else
                {
                    stream = new UnsupportedStream(uri);
                }
            }
            catch (ArgumentException)
            {
                stream = null;
                return false;
            }
            
            return true;
        }
    }
}
