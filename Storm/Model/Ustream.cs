using System;
using System.Configuration;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Storm.Extensions;

namespace Storm.Model
{
    public class Ustream : StreamBase
    {
        #region Fields
        private string channelId = string.Empty;
        #endregion

        #region Properties
        private readonly static BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Icons/Ustream.ico"));
        public override BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }
        #endregion

        public Ustream(Uri u)
            : base(u)
        {
            ApiUri = "https://api.ustream.tv";

            _icon.Freeze();
        }

        public async override Task UpdateAsync()
        {
            Updating = true;

            if (String.IsNullOrWhiteSpace(channelId))
            {
                channelId = await DetermineChannelIdAsync();
            }
            
            bool wasLive = IsLive;

            await DetermineIfLiveAsync();
            
            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive(nameof(Ustream));
            }

            Updating = false;
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/channels/{1}.json", ApiUri, channelId);
            HttpWebRequest request = BuildUstreamHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = false;

            if (json != null)
            {
                if (json.HasValues)
                {
                    if (HasUpdatedDisplayName == false)
                    {
                        SetDisplayName(json);
                    }
                    
                    live = ((string)json["channel"]["status"])
                        .Equals("live");
                }
            }
            
            IsLive = live;
        }

        private async Task<string> DetermineChannelIdAsync()
        {
            HttpWebRequest req = BuildUstreamHttpWebRequest(Uri);
            string response = await Utils.DownloadWebsiteAsStringAsync(req).ConfigureAwait(false);

            if (String.IsNullOrWhiteSpace(response))
            {
                return string.Empty;
            }
            
            string beginning = "\"channelId\":";
            string ending = ",";

            FromBetweenResult res = response.FromBetween(beginning, ending);
            
            if (res.Result == Result.Success)
            {
                return res.ResultValue;
            }

            return string.Empty;
        }

        private void SetDisplayName(JObject resp)
        {
            string displayName = (string)resp["channel"]["title"];

            if (String.IsNullOrEmpty(displayName) == false)
            {
                DisplayName = displayName;

                HasUpdatedDisplayName = true;
            }
        }
        
        private static HttpWebRequest BuildUstreamHttpWebRequest(Uri uri)
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

            req.Headers.Add("DNT", "1");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");

            return req;
        }
    }
}
