using System;

namespace Storm.Wpf.Streams
{
    public class YouTubeStream : StreamBase
    {
        protected override string ServiceName { get; } = "YouTube";

        private static readonly Uri _icon = new Uri($"{IconPackPrefix}YouTube.ico");
        public override Uri Icon => _icon;

        public YouTubeStream(Uri accountLink)
            : base(accountLink)
        { }
    }
}
