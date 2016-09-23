using System;
using System.Configuration;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Mixlr : StreamBase
    {
        #region Properties
        private readonly static BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Icons/Mixlr.ico"));
        public override BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }
        #endregion

        public Mixlr(Uri userUrl)
            : base(userUrl)
        {
            ApiUri = "https://api.mixlr.com/users";

            _icon.Freeze();
        }

        public override async Task UpdateAsync()
        {
            Updating = true;
            
            bool wasLive = IsLive;
            
            await DetermineIfLiveAsync();
            
            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive(nameof(Mixlr));
            }

            Updating = false;
        }
        
        protected override async Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/{1}", ApiUri, Name);
            HttpWebRequest request = BuildMixlrHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = false;
            
            if (json != null)
            {
                if (json.HasValues)
                {
                    if (HasUpdatedDisplayName == false)
                    {
                        TrySetDisplayName(json);
                    }

                    live = (bool)json["is_live"];
                }
            }
            
            IsLive = live;
        }

        private void TrySetDisplayName(JObject apiResp)
        {
            DisplayName = (string)apiResp["username"];

            if (DisplayName.Equals(Name) == false)
            {
                HasUpdatedDisplayName = true;
            }
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


// string apiAddressToQueryForStreams = string.Format("{0}/{1}?source=embed&callback=onUserLoad", apiUri, Name);
// var thing = (string)json["broadcasts"].FirstOrDefault()["streams"]["rtsp"]["url"];