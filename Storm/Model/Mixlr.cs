using System;
using System.Configuration;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    class Mixlr : StreamBase
    {
        public override string MouseOverTooltip
        {
            get
            {
                if (IsLive)
                {
                    return string.Format("{0} is live", DisplayName);
                }
                else
                {
                    return string.Format("{0} is offline", DisplayName);
                }
            }
        }

        public Mixlr(Uri userUrl)
            : base(userUrl)
        {
            apiUri = "http://api.mixlr.com/users";
            _isValid = true;
        }

        public override async Task UpdateAsync()
        {
            Updating = true;

            string apiAddressToQuery = string.Format("{0}/{1}", apiUri, Name);
            HttpWebRequest req = BuildMixlrHttpWebRequest(new Uri(apiAddressToQuery));

            JObject apiResp = await GetApiResponseAsync(req);

            if (apiResp.HasValues)
            {
                if (!hasUpdatedDisplayName)
                {
                    TrySetDisplayName(apiResp);
                }
                
                bool wasLive = IsLive;
                IsLive = apiResp["is_live"].Value<bool>();

                if (wasLive == false && IsLive == true)
                {
                    NotifyIsNowLive();
                }
            }

            Updating = false;
        }

        private void TrySetDisplayName(JObject apiResp)
        {
            DisplayName = apiResp["username"].Value<string>();

            if (DisplayName.Equals(Name) == false)
            {
                hasUpdatedDisplayName = true;
            }
        }

        private HttpWebRequest BuildMixlrHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri.AbsoluteUri);

            req.AllowAutoRedirect = true;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Timeout = 4000;
            req.UserAgent = ConfigurationManager.AppSettings["UserAgent"];

            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-encoding", "gzip, deflate");

            return req;
        }

        protected override Task DetermineIfLiveAsync()
        {
            throw new NotImplementedException();
        }

        protected override void NotifyIsNowLive()
        {
            string title = string.Format("{0} is now LIVE", DisplayName);
            Action action = () =>
                {
                    Utils.OpenUriInBrowser(this.Uri);
                };

            NotificationService.Send(title, action);
        }
    }
}
