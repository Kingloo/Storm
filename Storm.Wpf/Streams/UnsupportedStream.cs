using System;

namespace Storm.Wpf.Streams
{
    public class UnsupportedStream : StreamBase
    {
        public override Uri Icon { get; } = new Uri("pack://application:,,,/Icons/Unsupported.ico");

        public UnsupportedStream(Uri account)
            : base(account)
        { }

        protected override bool ValidateAccountLink() => true;

        protected override string SetAccountName() => AccountLink.AbsoluteUri;
    }
}
