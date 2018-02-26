using System;

namespace Storm.Model
{
    public static class StreamFactory
    {
        private static StringComparison sc = StringComparison.OrdinalIgnoreCase;

        public static bool TryCreate(string link, out StreamBase stream)
        {
            if (String.IsNullOrWhiteSpace(link))
            {
                stream = null;

                return false;
            }

            if (link.StartsWith("http://", sc) == false
                && link.StartsWith("https://", sc) == false)
            {
                link = string.Concat("http://", link);
            }

            if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uri))
            {
                stream = null;

                return false;
            }

            stream = DetermineStreamService(uri);

            return true;
        }

        private static StreamBase DetermineStreamService(Uri uri)
        {
            string dns = uri.DnsSafeHost;

            if (dns.EndsWith("twitch.tv", sc)) { return new Twitch(uri); }
            if (dns.EndsWith("ustream.tv", sc)) { return new Ustream(uri); }
            if (dns.EndsWith("mixlr.com", sc)) { return new Mixlr(uri); }
            if (dns.EndsWith("hitbox.tv", sc)) { return new Hitbox(uri); }
            if (dns.EndsWith("beam.pro", sc)) { return new Beam(uri); }
            if (dns.EndsWith("mixer.com", sc)) { return new Mixer(uri); }
            if (dns.EndsWith("chaturbate.com", sc)) { return new Chaturbate(uri); }
            if (dns.EndsWith("youtube.com", sc)) { return new YouTube(uri); }

            return new UnsupportedService(uri.AbsoluteUri);
        }
    }
}
