using System;
using System.Linq;

namespace Storm.Wpf.StreamServices.Twitch
{
    public class TwitchStream : StreamBase
    {
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
