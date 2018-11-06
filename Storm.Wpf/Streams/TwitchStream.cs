using System;
using System.Linq;
using System.Text;

namespace Storm.Wpf.Streams
{
    public class TwitchStream : StreamBase
    {
        public Int64 UserId { get; set; }
        public string Game { get; set; }

        public override Uri Icon { get; } = new Uri("pack://application:,,,/Icons/Twitch.ico");

        public TwitchStream(Uri account)
            : base(account)
        { }

        protected override bool ValidateAccountLink() => true;

        protected override string SetAccountName()
            => AccountLink.Segments.Last(segment => segment != "/");

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToString());

            sb.AppendLine(String.IsNullOrWhiteSpace(Game) ? "no game" : $"game: {Game}");

            return sb.ToString();
        }
    }
}
