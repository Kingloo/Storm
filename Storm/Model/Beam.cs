using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Beam : StreamBase
    {
        public override string MouseOverTooltip
        {
            get
            {
                return IsLive
                    ? string.Format("{0} is LIVE", DisplayName)
                    : string.Format("{0} is offline", DisplayName);
            }
        }

        public Beam(Uri accountUri) : base(accountUri)
        {
            ApiUri = "https://beam.pro/api/v1";
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
            string apiCall = string.Format("{0}/channels/{1}", ApiUri, Name);
            HttpWebRequest req = BuildBeamHttpWebRequest(new Uri(apiCall));

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp != null)
            {
                JToken tmpToken = null;

                if (!HasUpdatedDisplayName)
                {
                    if (resp.TryGetValue("token", out tmpToken))
                    {
                        DisplayName = (string)resp["token"];
                    }
                }

                if (resp.TryGetValue("online", out tmpToken))
                {
                    IsLive = (bool)resp["online"];
                }
                else
                {
                    IsLive = false;
                }
            }
        }

        private static HttpWebRequest BuildBeamHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

            req.Accept = "application/json; charset=UTF-8";
            req.AllowAutoRedirect = true;
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

            return req;
        }

        public override void GoToStream()
        {
            Utils.OpenUriInBrowser(Uri);
        }

        protected override void NotifyIsNowLive()
        {
            NotificationService.Send(string.Format(CultureInfo.CurrentCulture, "{0} is LIVE", DisplayName), () =>
            {
                Utils.OpenUriInBrowser(Uri);
            });
        }
    }
}
