using System.Linq;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            ApiUri = "https://api.mixlr.com/users";
            IsValid = true;
        }

        public override async Task UpdateAsync()
        {
            Updating = true;
            
            bool wasLive = IsLive;
            
            await DetermineIfLiveAsync();
            
            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive();
            }

            Updating = false;
        }
        
        protected override async Task DetermineIfLiveAsync()
        {
            // string apiAddressToQueryForStreams = string.Format("{0}/{1}?source=embed&callback=onUserLoad", apiUri, Name);
            // var thing = (string)json["broadcasts"].FirstOrDefault()["streams"]["rtsp"]["url"];

            string apiAddressToQuery = string.Format("{0}/{1}", ApiUri, Name);
            HttpWebRequest req = BuildMixlrHttpWebRequest(new Uri(apiAddressToQuery));

            JObject apiResp = await GetApiResponseAsync(req).ConfigureAwait(false);
            
            if (apiResp != null)
            {
                if (apiResp.HasValues)
                {
                    if (HasUpdatedDisplayName == false)
                    {
                        TrySetDisplayName(apiResp);
                    }

                    IsLive = (bool)apiResp["is_live"];

                    return;
                }
            }

            IsLive = false;
        }

        private void TrySetDisplayName(JObject apiResp)
        {
            DisplayName = (string)apiResp["username"];

            if (DisplayName.Equals(Name) == false)
            {
                HasUpdatedDisplayName = true;
            }
        }

        protected override void NotifyIsNowLive()
        {
            string title = string.Format("{0} is now LIVE", DisplayName);

            NotificationService.Send(title, () => Utils.OpenUriInBrowser(Uri));
        }

        public override void GoToStream()
        {
            Utils.OpenUriInBrowser(Uri);
        }

        private static HttpWebRequest BuildMixlrHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = WebRequest.CreateHttp(uri.AbsoluteUri);

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
    }
}
