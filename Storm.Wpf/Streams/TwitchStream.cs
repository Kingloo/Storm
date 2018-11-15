using System;
using System.Text;
using Storm.Wpf.Common;
using Storm.Wpf.StreamServices;

namespace Storm.Wpf.Streams
{
    public class TwitchStream : StreamBase
    {
        protected override string ServiceName { get; } = "Twitch";

        public override string MouseOverToolTip
        {
            get => String.IsNullOrWhiteSpace(Game)
                ? base.MouseOverToolTip
                : $"{DisplayName} is playing {Game}";
        }

        private static readonly Uri _icon = new Uri($"{IconPackPrefix}Twitch.ico");
        public override Uri Icon => _icon;

        public Int64 UserId { get; set; }
        public string Game { get; set; }

        public TwitchStream(Uri account)
            : base(account)
        { }

        protected override void NotifyLive()
        {
            string title = $"{DisplayName} is LIVE";
            string description = String.IsNullOrWhiteSpace(Game) ? string.Empty : $"and playing {Game}";

            Action startWatching = ServicesManager.StartWatching(this);

            NotificationService.Send(title, description, startWatching);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToString());

            sb.AppendLine(String.IsNullOrWhiteSpace(Game) ? "no game" : $"game: {Game}");

            return sb.ToString();
        }
    }
}
