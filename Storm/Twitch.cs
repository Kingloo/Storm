using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Storm.ViewModels;

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
        
        public Twitch(Uri u)
            : base(u)
        {
            this.apiUri = "https://api.twitch.tv/kraken";
        }

        public async override Task UpdateAsync()
        {
            Updating = true;

            List<Task> updateTasks = new List<Task>();

            if (!hasUpdatedDisplayName)
            {
                updateTasks.Add(TrySetDisplayNameAsync());
            }

            updateTasks.Add(DetermineGameAsync());
            updateTasks.Add(DetermineIfLiveAsync());

            bool wasLive = IsLive;

            await Task.WhenAll(updateTasks).ConfigureAwait(false);

            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive();
            }

            Updating = false;
        }

        protected async Task TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", this.apiUri, this._name);

            HttpWebRequest twitchRequest = BuildTwitchHttpWebRequest(
                new Uri(apiAddressToQueryForDisplayName)
                );

            JObject response = await GetApiResponseAsync(twitchRequest).ConfigureAwait(false);

            if (response != null)
            {
                if (response["display_name"] != null)
                {
                    this.DisplayName = (string)response["display_name"];

                    hasUpdatedDisplayName = true;
                }
            }
        }

        protected async Task DetermineGameAsync()
        {
            string apiAddressToQuery = string.Format("{0}/channels/{1}", this.apiUri, this._name);

            HttpWebRequest req = BuildTwitchHttpWebRequest(
                new Uri(apiAddressToQuery)
                );

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
            string apiAddressToQuery = string.Format("{0}/streams/{1}", this.apiUri, this._name);
            HttpWebRequest req = BuildTwitchHttpWebRequest(
                new Uri(apiAddressToQuery)
                );

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

            string title = string.Format("{0} is LIVE", this.DisplayName);

            if (String.IsNullOrWhiteSpace(this.Game))
            {
                showNotification = () => NotificationService.Send(title, new Action(() => MainWindowViewModel.GoToStream(this)));
            }
            else
            {
                string description = string.Format("and playing {0}", this.Game);

                showNotification = () => NotificationService.Send(title, description, new Action(() => MainWindowViewModel.GoToStream(this)));
            }

            Utils.SafeDispatcher(showNotification);
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
            req.Referer = string.Format("{0}://{1}", uri.GetLeftPart(UriPartial.Scheme), uri.DnsSafeHost);
            req.Timeout = 4000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

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
            sb.AppendLine(string.Format("Game: {0}", String.IsNullOrWhiteSpace(this.Game) ? "not set" : this.Game));

            return sb.ToString();
        }
    }
}
