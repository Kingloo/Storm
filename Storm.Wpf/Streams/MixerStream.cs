using System;

namespace Storm.Wpf.Streams
{
    public class MixerStream : StreamBase
    {
        public override Uri Icon { get; } = new Uri("pack://application:,,,/Icons/Mixer.ico");

        public MixerStream(Uri account)
            : base(account)
        { }

        protected override bool ValidateAccountLink() => true;

        protected override string SetAccountName()
        {
            return AccountLink.AbsoluteUri;
        }
    }
}
