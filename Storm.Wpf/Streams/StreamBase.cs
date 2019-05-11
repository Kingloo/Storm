using System;
using System.Linq;
using System.Text;
using Storm.Wpf.Common;
using Storm.Wpf.StreamServices;

namespace Storm.Wpf.Streams
{
    public abstract class StreamBase : BindableBase, IEquatable<StreamBase>
    {
        protected const string IconPackPrefix = "pack://application:,,,/Icons/";

        public virtual string MouseOverToolTip
        {
            get => IsLive
                ? $"{DisplayName} is LIVE"
                : $"{DisplayName} is offline";
        }

        protected abstract string ServiceName { get; }

        public Uri AccountLink { get; } = null;

        public abstract Uri Icon { get; }

        public string AccountName { get; protected set; } = string.Empty;

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(DisplayName), $"you tried to set {ServiceName}:{nameof(DisplayName)} to NullOrWhitespace");
                }

                SetProperty(ref _displayName, value, nameof(DisplayName));

                RaisePropertyChanged(nameof(MouseOverToolTip));
            }
        }

        private bool _isLive = false;
        public bool IsLive
        {
            get => _isLive;
            set
            {
                bool wasLive = IsLive;

                SetProperty(ref _isLive, value, nameof(IsLive));

                RaisePropertyChanged(nameof(MouseOverToolTip));

                // if they were not live before, but are live now
                // without this check it will NotifyLive() on every refresh
                // but you only want to be notified once
                if (!wasLive && IsLive)
                {
                    NotifyLive();

                    if (AutoRecord)
                    {
                        ServicesManager.StartRecording(this)?.Invoke();
                    }
                }
            }
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

        protected virtual bool ValidateAccountLink() => true;

        protected virtual string SetAccountName()
            => AccountLink.Segments.First(segment => segment != "/");

        protected virtual void NotifyLive()
        {
            string title = $"{DisplayName} is LIVE";
            string description = $"on {ServiceName}";

            Action startWatching = ServicesManager.StartWatching(this);

            NotificationService.Send(title, description, startWatching);
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
             We do this because the following illustrates all the different ways a URL could be supplied,
             and they must all lead to the same account object
                
                {http{s}://}{www.}twitch.tv/john{?query=blabla}
             */
            
            return thisMinimumLink.Equals(otherMinimumLink, StringComparison.CurrentCultureIgnoreCase);
        }

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
    }
}
