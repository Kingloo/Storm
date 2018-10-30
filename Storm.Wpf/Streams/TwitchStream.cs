using System;
using System.Linq;

namespace Storm.Wpf.Streams
{
    public class TwitchStream : StreamBase
    {
        public string Game { get; set; }

        public TwitchStream(Uri account)
            : base(account)
        { }

        protected override bool ValidateAccountLink()
        {
            return true;
        }

        protected override void SetAccountName()
            => AccountName = AccountLink.Segments.Last(segment => segment != "/");
    }
}
