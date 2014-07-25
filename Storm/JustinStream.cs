using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm
{
    class JustinStream : StreamBase
    {
        public JustinStream(string s)
            : base(s)
        {
            this._apiUri = "https://api.justin.tv/api";
        }

        public async override Task UpdateAsync()
        {
            if (!this._hasUpdatedDisplayName)
            {
                this._hasUpdatedDisplayName = await this.TrySetDisplayNameAsync();
            }

            await DetermineIfLive();
        }

        protected async override Task<bool> TrySetDisplayNameAsync()
        {
            string apiAddressToQueryForDisplayName = string.Format("{0}/channel/show/{1}.json", this._apiUri, this._name);
            HttpWebRequest justinRequest = BuildJustinHttpWebRequest(apiAddressToQueryForDisplayName);
            JObject response = await GetApiResponseAsync(justinRequest);

            if (response != null)
            {
                this.DisplayName = (string)response["title"];
                return true;
            }

            return false;
        }

        protected async override Task DetermineIfLive()
        {
            string apiAddressToQuery = string.Format("{0}/stream/summary.json?channel={1}", this._apiUri, this._name);
            HttpWebRequest req = BuildJustinHttpWebRequest(apiAddressToQuery);
            JObject resp = await GetApiResponseAsync(req);

            if (resp != null)
            {
                if (resp.HasValues)
                {
                    int streams_count = (int)resp["streams_count"];

                    if (streams_count > 0)
                    {
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
            }
        }

        private static HttpWebRequest BuildJustinHttpWebRequest(string fullApiRequestAddress)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(fullApiRequestAddress);

            req.KeepAlive = false;
            req.Method = "GET";
            req.Referer = "justin.tv";
            req.Timeout = 2000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            return req;
        }
    }
}
