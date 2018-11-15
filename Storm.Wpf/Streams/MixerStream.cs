using System;

namespace Storm.Wpf.Streams
{
    public class MixerStream : StreamBase
    {
        protected override string ServiceName { get; } = "Mixer";

        private static readonly Uri _icon = new Uri($"{IconPackPrefix}Mixer.ico");
        public override Uri Icon => _icon;

        public MixerStream(Uri account)
            : base(account)
        { }
    }
}
