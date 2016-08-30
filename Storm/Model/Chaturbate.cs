using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;

namespace Storm.Model
{
    public class Chaturbate : StreamBase
    {
        public override string MouseOverTooltip
        {
            get
            {
                return IsLive
                    ? string.Format(CultureInfo.CurrentCulture, "{0} is live", DisplayName)
                    : string.Format(CultureInfo.CurrentCulture, "{0} is offline", DisplayName);
            }
        }

        public Chaturbate(Uri accountUri) : base(accountUri) { }

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
            HttpWebRequest req = BuildHttpWebRequest(Uri);

            string website = await Utils.DownloadWebsiteAsStringAsync(req);

            if (String.IsNullOrWhiteSpace(website) == false)
            {
                // notice the negation
                // if the website string contains "Room is currently offline"
                // then the stream is offline
                // therefore IsLive would be false

                IsLive = !website.Contains("Room is currently offline");
            }
        }
        
        protected override void NotifyIsNowLive()
        {
            string title = string.Format(CultureInfo.CurrentCulture, "{0} is LIVE", DisplayName);

            NotificationService.Send(title, () => Utils.OpenUriInBrowser(Uri));
        }
        
        private static HttpWebRequest BuildHttpWebRequest(Uri uri)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp(uri);

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
