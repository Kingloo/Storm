using System;

namespace Storm.Wpf.Streams
{
    public class MixerStream : StreamBase
    {
        private static readonly Uri _icon = new Uri("pack://application:,,,/Icons/Mixer.ico");
        public override Uri Icon => _icon;

        public MixerStream(Uri account)
            : base(account)
        { }
    }
}
