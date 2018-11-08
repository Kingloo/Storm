using System;
using System.Text;

namespace Storm.Wpf.Streams
{
    public class TwitchStream : StreamBase
    {
        public Int64 UserId { get; set; }
        public string Game { get; set; }

        private static readonly Uri _icon = new Uri($"{IconPackUriPrefix}Twitch.ico");
        public override Uri Icon => _icon;

        public TwitchStream(Uri account)
            : base(account)
        { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToString());

            sb.AppendLine(String.IsNullOrWhiteSpace(Game) ? "no game" : $"game: {Game}");

            return sb.ToString();
        }
    }
}
