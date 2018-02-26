using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Storm.Common;

namespace Storm.Model
{
    public class Twitch : StreamBase
    {
        #region Properties
        private string _game = string.Empty;
        public string Game
        {
            get => _game;
            set
            {
                if (_game != value)
                {
                    _game = value;

                    RaisePropertyChanged(nameof(Game));
                    RaisePropertyChanged(nameof(MouseOverTooltip));
                }
            }
        }

        public override string MouseOverTooltip
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                
                sb.Append(DisplayName);
                sb.Append(" is ");
                
                if (IsLive)
                {
                    if (String.IsNullOrWhiteSpace(Game))
                    {
                        sb.Append("LIVE");
                    }
                    else
                    {
                        sb.Append($"playing ");
                        sb.Append(Game);
                    }
                }
                else
                {
                    sb.Append($"offline");
                }

                return sb.ToString();
            }
        }

        public override Uri Api => new Uri("https://api.twitch.tv/kraken");

        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Twitch.ico"));
        public override BitmapImage Icon => _icon;

        public override bool HasStreamlinkSupport => true;
        #endregion

        public Twitch(Uri uri)
            : base(uri)
        { }

        public async override Task UpdateAsync()
        {
            Updating = true;

            var updateTasks = new List<Task>();

            bool wasLive = IsLive;

            if (!HasUpdatedDisplayName)
            {
                updateTasks.Add(TrySetDisplayNameAsync());
            }

            updateTasks.Add(DetermineGameAsync());
            updateTasks.Add(DetermineIfLiveAsync());
            
            await Task.WhenAll(updateTasks);

            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(Twitch));
            }

            Updating = false;
        }

        protected async Task TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = $"{Api.AbsoluteUri}/channels/{Name}";

            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQueryForDisplayName));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true)
                .ConfigureAwait(false));

            if (json != null)
            {
                if (json["display_name"] != null)
                {
                    DisplayName = (string)json["display_name"];

                    HasUpdatedDisplayName = true;
                }
            }
        }

        protected async Task DetermineGameAsync()
        {
            string apiAddressToQuery = $"{Api.AbsoluteUri}/channels/{Name}";

            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            if (json != null)
            {
                if (json["game"] is JToken token)
                {
                    Game = (string)token;
                }
            }
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = $"{Api.AbsoluteUri}/streams/{Name}";

            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));

            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = IsLive;

            if (json != null)
            {
                if (json["stream"] is JToken token)
                {
                    live = token.HasValues;
                }
            }
            
            IsLive = live;
        }

        public override void NotifyIsNowLive(string serviceName)
        {
            string title = string.Format(CultureInfo.CurrentCulture, "{0} is LIVE", DisplayName);
            
            if (String.IsNullOrWhiteSpace(Game))
            {
                NotificationService.Send(title, GoToStream);
            }
            else
            {
                string description = string.Format(CultureInfo.CurrentCulture, "and playing {0}", Game);
                
                NotificationService.Send(title, description, GoToStream);
            }
        }
        
        protected override HttpWebRequest BuildHttpWebRequest(Uri uri)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            HttpWebRequest req = base.BuildHttpWebRequest(uri);

            req.Accept = "application/vnd.twitchtv.v3+json";
            req.Headers.Add("Client-ID", "ewvlchtxgqq88ru9gmfp1gmyt6h2b93");

            // jzkbprff40iqj646a697cyrvl0zt2m6
            // lf8xspujnqfqcdlj11zq77dfen2tqjo
            // pwkzresl8kj2rdj6g7bvxl9ys1wly3j

            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToString());
            sb.Append("Game: ");
            sb.Append(String.IsNullOrWhiteSpace(Game) ? "not set" : Game);

            return sb.ToString();
        }
    }
}
