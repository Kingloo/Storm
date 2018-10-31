using System;
using System.Text;
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

            AccountName = SetAccountName().ToLower();
        }

        protected abstract bool ValidateAccountLink();
        protected abstract string SetAccountName();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().FullName);
            sb.AppendLine($"account link: {AccountLink.AbsoluteUri}");
            sb.AppendLine(AccountIcon == null ? "no account icon" : $"account icon: {AccountIcon.AbsoluteUri}");
            sb.AppendLine($"account name: {AccountName}");
            sb.AppendLine($"display name: {DisplayName}");
            sb.AppendLine(IsLive ? "live" : "not live");

            return sb.ToString();
        }
    }
}
