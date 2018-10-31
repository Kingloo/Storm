using System;

namespace Storm.Wpf.Streams
{
    public static class StreamFactory
    {
        private const string http = "http://";
        private const string https = "https://";

        /// <summary>
        /// Attempts to create the correct stream object from a line of text.
        /// </summary>
        /// <param name="rawAccountLink">The account link as raw text.</param>
        /// <param name="stream">If succcessful, the IStream object, e.g. TwitchStream or ChaturbateStream.</param>
        /// <returns>Will fail for several reasons, including if the line was a comment, or didn't parse as a Uri.</returns>
        public static bool TryCreate(string rawAccountLink, out IStream stream)
        {
            // comment character, each comment must be on its own line
            if (rawAccountLink.StartsWith("/"))
            {
                stream = null;
                return false;
            }

            // we don't replace http:// with https:// - we leave it up to the services to do 301 redirects

            if (!rawAccountLink.StartsWith(https) && !rawAccountLink.StartsWith(http))
            {
                rawAccountLink = $"{https}{rawAccountLink}";
            }

            if (!Uri.TryCreate(rawAccountLink, UriKind.Absolute, out Uri uri))
            {
                stream = null;
                return false;
            }

            string host = uri.DnsSafeHost;
            var sc = StringComparison.InvariantCultureIgnoreCase;

            try
            {
                if (host.EndsWith("twitch.tv", sc)) { stream = new TwitchStream(uri); }
                else if (host.EndsWith("chaturbate.com", sc)) { stream = new ChaturbateStream(uri); }
                else
                {
                    stream = null;
                    return false;
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
