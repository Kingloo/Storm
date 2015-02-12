using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm
{
    class Ustream : StreamBase
    {
        #region Fields
        private string channelId = string.Empty;
        #endregion

        #region Properties
        public override string MouseOverTooltip
        {
            get
            {
                if (this.IsLive)
                {
                    return string.Format("{0} is live", this.DisplayName);
                }
                else
                {
                    return string.Format("{0} is offline", this.DisplayName);
                }
            }
        }
        #endregion

        public Ustream(string s)
            : base(s)
        {
            this._apiUri = "https://api.ustream.tv";
        }

        public async override Task UpdateAsync()
        {
            this.Updating = true;

            bool userIsLive = await DetermineIfLive();

            if (userIsLive)
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

            this.Updating = false;
        }

        protected async override Task<bool> DetermineIfLive()
        {
            if (String.IsNullOrEmpty(channelId))
            {
                channelId = await DetermineChannelId();
            }

            string apiAddressToQuery = string.Format("{0}/channels/{1}.json", this._apiUri, channelId);
            HttpWebRequest req = BuildUstreamHttpWebRequest(new Uri(apiAddressToQuery));

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                if (resp["channel"].HasValues)
                {
                    if (this._hasUpdatedDisplayName == false)
                    {
                        SetDisplayName(resp);
                    }

                    return WasUserLive(resp);
                }
            }
            else
            {
                if (req != null)
                {
                    req.Abort();
                }
            }

            return false;
        }

        private async Task<string> DetermineChannelId()
        {
            HttpWebRequest req = BuildUstreamHttpWebRequest(this.Uri);
            string response = await Utils.DownloadWebsiteAsStringAsync(req);

            if (String.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            else
            {
                string beginning = "<param name=\"flashvars\" value=\"cid=";
                string ending = "&amp;locale=";

                channelId = response.FromBetween(beginning, ending);
                
                if (string.IsNullOrWhiteSpace(channelId))
                {
                    return null;
                }
                else
                {
                    return channelId;
                }
            }
        }

        private void SetDisplayName(JObject resp)
        {
            string displayName = (string)resp["channel"]["title"];

            if (String.IsNullOrEmpty(displayName) == false)
            {
                this.DisplayName = displayName;

                this._hasUpdatedDisplayName = true;
            }
        }

        private bool WasUserLive(JObject resp)
        {
            string statusValue = (string)resp["channel"]["status"];

            if (String.IsNullOrEmpty(statusValue) == false)
            {
                if (statusValue.Equals("live"))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void NotifyIsNowLive()
        {
            string title = string.Format("{0} is now live", this.DisplayName);

            NotificationService.Send(title, this.Uri);
        }

        private static HttpWebRequest BuildUstreamHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

            req.Accept = "application/json; charset=UTF-8";
            req.AllowAutoRedirect = true;
            req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Referer = string.Format("{0}://{1}", uri.GetLeftPart(UriPartial.Scheme), uri.DnsSafeHost);
            req.Timeout = 4000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

            if (uri.Scheme.Equals("https"))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            req.Headers.Add("DNT", "1");

            return req;
        }
    }
}
