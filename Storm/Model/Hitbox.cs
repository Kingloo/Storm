using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    class Hitbox : StreamBase
    {
        public override string MouseOverTooltip
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} is LIVE on Hitbox", DisplayName);
            }
        }

        public Hitbox(Uri accountUri) : base(accountUri)
        {
            ApiUri = "https://api.hitbox.tv";
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
            string apiAddressToQuery = string.Format("{0}/user/{1}", ApiUri, Name);
            HttpWebRequest req = BuildHitboxHttpWebRequest(new Uri(apiAddressToQuery));

            JObject apiResp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (apiResp != null)
            {
                if (apiResp.HasValues)
                {
                    bool wasLive = IsLive;

                    IsLive = Convert.ToBoolean((int)apiResp["is_live"]);

                    return;
                }
            }

            IsLive = false;
        }

        protected override void NotifyIsNowLive()
        {
            string title = string.Format(CultureInfo.CurrentCulture, "{0} is LIVE on Hitbox", DisplayName);
            
            NotificationService.Send(title, GoToStream);
        }
        
        private static HttpWebRequest BuildHitboxHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = WebRequest.CreateHttp(uri);

            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            req.Host = uri.DnsSafeHost;
            req.KeepAlive = false;
            req.Method = "GET";
            req.ProtocolVersion = HttpVersion.Version11;
            req.Timeout = 2500;
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
