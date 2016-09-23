using System;
using System.Configuration;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Hitbox : StreamBase
    {
        #region Properties
        private readonly static BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Icons/Hitbox.ico"));
        public override BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }
        #endregion

        public Hitbox(Uri accountUri) : base(accountUri)
        {
            ApiUri = "https://api.hitbox.tv";

            _icon.Freeze();
        }
        
        public override async Task UpdateAsync()
        {
            Updating = true;
            
            bool wasLive = IsLive;

            await DetermineIfLiveAsync();

            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive(nameof(Hitbox));
            }

            Updating = false;
        }
        
        protected override async Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/user/{1}", ApiUri, Name);
            HttpWebRequest request = BuildHitboxHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = false;

            if (json != null)
            {
                if (json.HasValues)
                {
                    live = Convert.ToBoolean((int)json["is_live"]);
                }
            }
            
            IsLive = live;
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
