using System;
using System.Linq;

namespace Storm.Wpf.Streams
{
    public class ChaturbateStream : StreamBase
    {
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
