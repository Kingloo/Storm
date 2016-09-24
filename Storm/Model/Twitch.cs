using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Twitch : StreamBase
    {
        #region Properties
        private string _game = string.Empty;
        public string Game
        {
            get
            {
                return _game;
            }
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
                if (IsLive)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format(CultureInfo.CurrentCulture, "{0} is LIVE", DisplayName));

                    if (String.IsNullOrWhiteSpace(Game) == false)
                    {
                        sb.Append(string.Format(CultureInfo.CurrentCulture, " and playing {0}", Game));
                    }

                    return sb.ToString();
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} is offline", DisplayName);
                }
            }
        }

        private readonly static BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Icons/Twitch.ico"));
        public override BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }
        #endregion

        public Twitch(Uri u)
            : base(u)
        {
            ApiUri = "https://api.twitch.tv/kraken";
            HasLivestreamerSupport = true;

            _icon.Freeze();
        }

        public async override Task UpdateAsync()
        {
            Updating = true;

            List<Task> updateTasks = new List<Task>();

            bool wasLive = IsLive;

            if (!HasUpdatedDisplayName)
            {
                updateTasks.Add(TrySetDisplayNameAsync());
            }

            updateTasks.Add(DetermineGameAsync());
            updateTasks.Add(DetermineIfLiveAsync());
            
            await Task.WhenAll(updateTasks);

            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive(nameof(Twitch));
            }

            Updating = false;
        }

        protected async Task TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", ApiUri, Name);
            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQueryForDisplayName));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

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
            string apiAddressToQuery = string.Format("{0}/channels/{1}", ApiUri, Name);
            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            if (json != null)
            {
                if (json["game"] is JToken)
                {
                    Game = (string)json["game"];
                }
            }
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/streams/{1}", ApiUri, Name);
            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));

            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = false;

            if (json != null)
            {
                if (json["stream"] != null)
                {
                    if (json["stream"].HasValues)
                    {
                        live = true;
                    }
                }
            }
            
            IsLive = live;
        }

        public override void NotifyIsNowLive(string _)
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

            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToString());
            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "Game: {0}", String.IsNullOrWhiteSpace(Game) ? "not set" : Game));

            return sb.ToString();
        }
    }
}
