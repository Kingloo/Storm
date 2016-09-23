using System;
using System.Configuration;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Beam : StreamBase
    {
        #region Properties
        private readonly static BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Icons/Beam.ico"));
        public override BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }
        #endregion

        public Beam(Uri accountUri) : base(accountUri)
        {
            ApiUri = "https://beam.pro/api/v1";

            _icon.Freeze();
        }
        
        public override async Task UpdateAsync()
        {
            Updating = true;

            bool wasLive = IsLive;
            
            await DetermineIfLiveAsync();

            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive(nameof(Beam));
            }

            Updating = false;
        }
        
        protected override async Task DetermineIfLiveAsync()
        {
            string apiCall = string.Format("{0}/channels/{1}", ApiUri, Name);
            HttpWebRequest request = BuildBeamHttpWebRequest(new Uri(apiCall));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));
            
            bool live = false;
            
            if (json != null)
            {
                JToken tmpToken = null;

                if (!HasUpdatedDisplayName)
                {
                    if (json.TryGetValue("token", out tmpToken))
                    {
                        DisplayName = (string)json["token"];
                    }
                }

                if (json.TryGetValue("online", out tmpToken))
                {
                    live = (bool)json["online"];
                }
            }
            
            IsLive = live;
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
    }
}
