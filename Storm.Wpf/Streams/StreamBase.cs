using System;
using System.Diagnostics;
using System.Text;
using Storm.Wpf.Common;

namespace Storm.Wpf.Streams
{
    public abstract class StreamBase : BindableBase, IStream
    {
        public Uri AccountLink { get; } = null;

        private Uri _accountIcon = null;
        public Uri AccountIcon
        {
            get => _accountIcon;
            set => SetProperty(ref _accountIcon, value, nameof(AccountIcon));
        }

        public string AccountName { get; protected set; } = string.Empty;

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value, nameof(DisplayName));
        }

        private bool _isLive = false;
        public bool IsLive
        {
            get => _isLive;
            set => SetProperty(ref _isLive, value, nameof(IsLive));
        }

        private bool _autoRecord = false;
        public bool AutoRecord
        {
            get => _autoRecord;
            set => SetProperty(ref _autoRecord, value, nameof(AutoRecord));
        }

        protected StreamBase(Uri accountLink)
        {
            AccountLink = accountLink ?? throw new ArgumentNullException(nameof(accountLink));

            if (!ValidateAccountLink())
            {
                throw new ArgumentException($"account link invalid: {accountLink.AbsoluteUri}", nameof(accountLink));
            }

            AccountName = SetAccountName().ToLower();
            DisplayName = AccountName;
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
