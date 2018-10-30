using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storm.Wpf.StreamServices
{
    public abstract class StreamBase : IStream
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
