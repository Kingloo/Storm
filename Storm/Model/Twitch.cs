using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    class Twitch : StreamBase
    {
        private string _game = string.Empty;
        public string Game
        {
            get
            {
                return _game;
            }
            set
            {
                _game = value;

                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("MouseOverTooltip");
            }
        }

        public override string MouseOverTooltip
        {
            get
            {
                if (IsLive)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format(CultureInfo.CurrentCulture, "{0} is live", DisplayName));

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
        
        public Twitch(Uri u)
            : base(u)
        {
            ApiUri = "https://api.twitch.tv/kraken";
            HasLivestreamerSupport = false;
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
                NotifyIsNowLive();
            }

            Updating = false;
        }

        protected async Task TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", ApiUri, Name);
            HttpWebRequest twitchRequest = BuildTwitchHttpWebRequest(new Uri(apiAddressToQueryForDisplayName));

            JObject response = await GetApiResponseAsync(twitchRequest).ConfigureAwait(false);

            if (response != null)
            {
                if (response["display_name"] != null)
                {
                    DisplayName = (string)response["display_name"];

                    HasUpdatedDisplayName = true;
                }
            }
        }

        protected async Task DetermineGameAsync()
        {
            string apiAddressToQuery = string.Format("{0}/channels/{1}", ApiUri, Name);
            HttpWebRequest req = BuildTwitchHttpWebRequest(new Uri(apiAddressToQuery));

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                if (resp["game"] is JToken)
                {
                    Game = (string)resp["game"];
                }
            }
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/streams/{1}", ApiUri, Name);
            HttpWebRequest req = BuildTwitchHttpWebRequest(new Uri(apiAddressToQuery));

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                if (resp["stream"] != null)
                {
                    if (resp["stream"].HasValues)
                    {
                        IsLive = true;

                        return;
                    }
                }
            }

            IsLive = false;
        }

        protected override void NotifyIsNowLive()
        {
            Action showNotification = null;

            string title = string.Format(CultureInfo.CurrentCulture, "{0} is LIVE", DisplayName);

            if (String.IsNullOrWhiteSpace(Game))
            {
                showNotification = () => NotificationService.Send(title, GoToStream);
            }
            else
            {
                string description = string.Format(CultureInfo.CurrentCulture, "and playing {0}", Game);
                
                showNotification = () => NotificationService.Send(title, description, GoToStream);
            }
            
            showNotification();
        }

        public override void GoToStream()
        {
            string uri = string.Format("https://player.twitch.tv/?branding=false&showInfo=false&channel={0}", Name);
            
            Utils.OpenUriInBrowser(new Uri(uri));
        }

        private static HttpWebRequest BuildTwitchHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

            req.Accept = "application/vnd.twitchtv.v3+json";
            req.AllowAutoRedirect = true;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = string.Format(CultureInfo.InvariantCulture, "{0}{1}", uri.GetLeftPart(UriPartial.Scheme), uri.DnsSafeHost);
            req.Timeout = 4000;
            req.UserAgent = ConfigurationManager.AppSettings["UserAgent"];

            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");

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
