using System;

namespace Storm.Wpf.Streams
{
    public class YouTubeStream : StreamBase
    {
        private static readonly Uri _icon = new Uri($"{IconPackUriPrefix}YouTube.ico");
        public override Uri Icon => _icon;

        public YouTubeStream(Uri accountLink)
            : base(accountLink)
        { }
    }
}
