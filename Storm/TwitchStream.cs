using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm
{
    class TwitchStream : StreamBase
    {
        public TwitchStream(string s)
            : base(s)
        {
            this._apiUri = "https://api.twitch.tv/kraken";
        }

        public async override Task UpdateAsync()
        {
            if (!this._hasUpdatedDisplayName)
            {
                this._hasUpdatedDisplayName = await TrySetDisplayNameAsync();
            }

            bool isUserLive = await DetermineIfLive();
            
            if (isUserLive)
            {
                this.Game = await DetermineGame();

                if (this.IsLive == false)
                {
                    this.IsLive = true;

                    this.NotifyIsNowLive();
                }
            }
            else
            {
                if (this.IsLive == true)
                {
                    this.IsLive = false;
                }
            }
        }

        protected async override Task<bool> TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", this._apiUri, this._name);
            HttpWebRequest twitchRequest = BuildTwitchHttpWebRequest(new Uri(apiAddressToQueryForDisplayName));

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
            HttpWebRequest req = BuildTwitchHttpWebRequest(new Uri(apiAddressToQuery));

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                if (resp["game"] is JToken)
                {
                    return ((string)resp["game"]) ?? "unknown";
                }
            }

            return "unknown";
        }

        protected async override Task<bool> DetermineIfLive()
        {
            string apiAddressToQuery = string.Format("{0}/streams/{1}", this._apiUri, this._name);
            HttpWebRequest req = BuildTwitchHttpWebRequest(new Uri(apiAddressToQuery));

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

        private static HttpWebRequest BuildTwitchHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

            req.Accept = ("application/vnd.twitchtv.v2+json");
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.Referer = uri.DnsSafeHost;
            req.Timeout = 850;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            return req;
        }
    }
}
