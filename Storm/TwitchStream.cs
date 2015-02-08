using System;
using System.Net;
using System.Text;
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
            req.AllowAutoRedirect = true;
            req.AutomaticDecompression = DecompressionMethods.GZip;
            req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = string.Format("{0}://{1}", uri.GetLeftPart(UriPartial.Scheme), uri.DnsSafeHost);
            req.Timeout = 4000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding: gzip");

            return req;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.GetType().ToString());
            sb.AppendLine(string.Format("API Uri: {0}", this._apiUri));
            sb.AppendLine(string.Format("Uri: {0}", this.Uri));
            sb.AppendLine(string.Format("Name: {0}", this.Name));
            sb.AppendLine(string.Format("DisplayName: {0}", this.DisplayName));
            sb.AppendLine(string.Format("Is live? {0}", this.IsLive));
            sb.AppendLine(string.Format("Game: {0}", this.Game));
            sb.AppendLine(string.Format("MouseOverTooltip: {0}", this.MouseOverTooltip));

            return sb.ToString();
        }
    }
}
