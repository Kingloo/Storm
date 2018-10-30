using System;

namespace Storm.Wpf.Streams
{
    public static class StreamFactory
    {
        public static bool TryCreate(string rawAccountLink, out IStream stream)
        {
            if (!Uri.TryCreate(rawAccountLink, UriKind.Absolute, out Uri uri))
            {
                stream = null;
                return false;
            }

            string host = uri.DnsSafeHost;
            var sc = StringComparison.InvariantCultureIgnoreCase;

            if (host.EndsWith("twitch.tv", sc)) { stream = new TwitchStream(uri); }
            //else if (host.EndsWith("chaturbate.com", sc)) { stream = new ChaturbateStream(uri); }
            else
            {
                stream = null;
                return false;
            }

            return true;
        }
    }
}
