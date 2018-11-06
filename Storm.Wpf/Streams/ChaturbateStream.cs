using System;
using System.Linq;

namespace Storm.Wpf.Streams
{
    public class ChaturbateStream : StreamBase
    {
        private static readonly Uri _icon = new Uri("pack://application:,,,/Icons/Chaturbate.ico");
        public override Uri Icon => _icon;

        public ChaturbateStream(Uri account)
            : base(account)
        { }

        protected override bool ValidateAccountLink()
        {
            return true;
        }

        protected override string SetAccountName()
            => AccountLink.Segments.Last(segment => segment != "/");
    }
}
