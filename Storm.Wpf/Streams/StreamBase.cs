using System;
using Storm.Wpf.Common;

namespace Storm.Wpf.Streams
{
    public abstract class StreamBase : BindableBase, IStream
    {
        public Uri AccountLink { get; } = null;
        public Uri AccountIcon { get; set; } = null;
        public string AccountName { get; protected set; } = string.Empty;
        public string DisplayName { get; set; }
        public bool IsLive { get; set; }

        protected StreamBase(Uri accountLink)
        {
            AccountLink = accountLink ?? throw new ArgumentNullException(nameof(accountLink));

            if (!ValidateAccountLink())
            {
                throw new ArgumentException($"account link invalid: {accountLink.AbsoluteUri}", nameof(accountLink));
            }

            SetAccountName();
        }

        protected abstract bool ValidateAccountLink();
        protected abstract void SetAccountName();
    }
}
