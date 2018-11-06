using System;

namespace Storm.Wpf.Streams
{
    public class UnsupportedStream : StreamBase
    {
        private static readonly Uri _icon = new Uri("pack://application:,,,/Icons/Unsupported.ico");
        public override Uri Icon => _icon;

        public UnsupportedStream(Uri account)
            : base(account)
        { }

        protected override bool ValidateAccountLink() => true;

        protected override string SetAccountName() => AccountLink.AbsoluteUri;
    }
}
