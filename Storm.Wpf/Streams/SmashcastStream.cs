using System;

namespace Storm.Wpf.Streams
{
    public class SmashcastStream : StreamBase
    {
        private static readonly Uri _icon = new Uri($"{IconPackUriPrefix}Smashcast.ico");
        public override Uri Icon => _icon;

        public SmashcastStream(Uri accountLink)
            : base(accountLink)
        { }
    }
}
