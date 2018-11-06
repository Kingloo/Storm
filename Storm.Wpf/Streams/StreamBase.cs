using System;
using System.Text;
using Storm.Wpf.Common;

namespace Storm.Wpf.Streams
{
    public abstract class StreamBase : BindableBase, IStream, IEquatable<StreamBase>
    {
        public Uri AccountLink { get; } = null;

        public abstract Uri Icon { get; }

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
            sb.AppendLine(Icon == null ? "no account icon" : $"account icon: {Icon.AbsoluteUri}");
            sb.AppendLine($"account name: {AccountName}");
            sb.AppendLine($"display name: {DisplayName}");
            sb.AppendLine(IsLive ? "live" : "not live");

            return sb.ToString();
        }

        public bool Equals(StreamBase other)
        {
            if (other is null)
            {
                return false;
            }

            string thisMinimumLink = $"{AccountLink.DnsSafeHost}{AccountLink.AbsolutePath}";
            string otherMinimumLink = $"{other.AccountLink.DnsSafeHost}{other.AccountLink.AbsolutePath}";

            /*
             We do this because all of the following URLs lead to the same account
                
                https://twitch.tv/john
                https://www.twitch.tv/john
                
                http://twitch.tv/john
                http://www.twitch.tv/john
                
                twitch.tv/john
                www.twitch.tv/john
                
                twitch.tv/john?extra=something
                www.twitch.tv/john?extra=something
                
            but Uri.Equals would find all of these to be different from each other
             */
            
            return thisMinimumLink.Equals(otherMinimumLink, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
