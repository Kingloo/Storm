using System;

namespace Storm.Wpf.Streams
{
    public class SmashcastStream : StreamBase
    {
        protected override string ServiceName { get; } = "Smashcast";

        private static readonly Uri _icon = new Uri($"{IconPackPrefix}Smashcast.ico");
        public override Uri Icon => _icon;

        public SmashcastStream(Uri accountLink)
            : base(accountLink)
        { }
    }
}
