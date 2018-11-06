using System;
using System.Linq;

namespace Storm.Wpf.Streams
{
    public class ChaturbateStream : StreamBase
    {
        public override Uri Icon { get; } = new Uri("pack://application:,,,/Icons/Chaturbate.ico");

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
