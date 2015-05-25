using System;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace Storm
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
                if (this.IsLive)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format("{0} is live", this.DisplayName));

                    if (String.IsNullOrWhiteSpace(this.Game) == false)
                    {
                        sb.Append(string.Format(" and playing {0}", this.Game));
                    }

                    return sb.ToString();
                }
                else
                {
                    return string.Format("{0} is offline", this.DisplayName);
                }
            }
        }
        
        public Twitch(string s)
            : base(s)
        {
            this._apiUri = "https://api.twitch.tv/kraken";
        }

        public async override Task UpdateAsync()
        {
            Updating = true;
            
            if (!_hasUpdatedDisplayName)
            {
                _hasUpdatedDisplayName = await TrySetDisplayNameAsync();
            }

            bool isUserLive = await DetermineIfLive();
            
            if (isUserLive)
            {
                Game = await DetermineGame();

                if (IsLive == false)
                {
                    IsLive = true;

                    NotifyIsNowLive();
                }
            }
            else
            {
                if (IsLive == true)
                {
                    IsLive = false;
                }
            }

            this.Updating = false;
        }

        protected async Task<bool> TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", this._apiUri, this._name);

            HttpWebRequest twitchRequest = BuildTwitchHttpWebRequest(
                new Uri(apiAddressToQueryForDisplayName)
                );

            JObject response = await GetApiResponseAsync(twitchRequest).ConfigureAwait(false);

            if (response != null)
            {
                if (response["display_name"] != null)
                {
                    this.DisplayName = (string)response["display_name"];

                    return true;
                }
            }

            return false;
        }

        protected async Task<string> DetermineGame()
        {
            string apiAddressToQuery = string.Format("{0}/channels/{1}", this._apiUri, this._name);

            HttpWebRequest req = BuildTwitchHttpWebRequest(
                new Uri(apiAddressToQuery)
                );

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                if (resp["game"] is JToken)
                {
                    return (string)resp["game"];
                }
            }

            return string.Empty;
        }

        protected async override Task<bool> DetermineIfLive()
        {
            string apiAddressToQuery = string.Format("{0}/streams/{1}", this._apiUri, this._name);
            HttpWebRequest req = BuildTwitchHttpWebRequest(
                new Uri(apiAddressToQuery)
                );

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                if (resp["stream"] is JToken)
                {
                    if (resp["stream"].HasValues)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void NotifyIsNowLive()
        {
            Action showNotification = null;

            string title = string.Format("{0} is LIVE", this.DisplayName);

            if (String.IsNullOrWhiteSpace(this.Game))
            {
                showNotification = () => NotificationService.Send(title, this.Uri);
            }
            else
            {
                string description = string.Format("and playing {0}", this.Game);

                showNotification = () => NotificationService.Send(title, description, this.Uri);
            }

            Utils.SafeDispatcher(
                showNotification,
                DispatcherPriority.Background
                );
        }

        private static HttpWebRequest BuildTwitchHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

            //req.Accept = ("application/vnd.twitchtv.v2+json");
            req.Accept = "application/vnd.twitchtv.v3+json";
            req.AllowAutoRedirect = true;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = string.Format("{0}://{1}", uri.GetLeftPart(UriPartial.Scheme), uri.DnsSafeHost);
            req.Timeout = 4000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");

            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToString());
            sb.AppendLine(string.Format("Game: {0}", String.IsNullOrWhiteSpace(this.Game) ? "not set" : this.Game));

            return sb.ToString();
        }
    }
}
