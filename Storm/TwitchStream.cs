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

            string streamApiAddress = string.Format("{0}/streams/{1}", this._apiUri, this._name);

            HttpWebRequest updateRequest = BuildTwitchHttpWebRequest(streamApiAddress);
            JObject apiResponse = await GetApiResponseAsync(updateRequest);

            if (apiResponse != null)
            {
                ProcessApiResponse(apiResponse);
            }
        }

        protected async override Task<bool> TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channels/{1}", this._apiUri, this._name);
            HttpWebRequest twitchRequest = BuildTwitchHttpWebRequest(apiAddressToQueryForDisplayName);
            JObject response = await GetApiResponseAsync(twitchRequest);

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

        private static HttpWebRequest BuildTwitchHttpWebRequest(string fullApiRequestAddress)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(fullApiRequestAddress);

            req.Accept = ("application/vnd.twitchtv.v2+json");
            req.KeepAlive = false;
            req.Method = "GET";
            req.Referer = "twitch.tv";
            req.Timeout = 2500;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            return req;
        }

        protected override void ProcessApiResponse(JObject jobj)
        {
            if (jobj["stream"] is JToken)
            {
                if (jobj["stream"].HasValues)
                {
                    if (this.IsLive == false)
                    {
                        this.IsLive = true;

                        this.OnHasGoneLive(this);
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
        }
    }
}
