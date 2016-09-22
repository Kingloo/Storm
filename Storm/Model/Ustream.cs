using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Storm.Extensions;

namespace Storm.Model
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
                if (IsLive)
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} is LIVE on Ustream", DisplayName);
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} is offline", DisplayName);
                }
            }
        }
        #endregion

        public Ustream(Uri u)
            : base(u)
        {
            ApiUri = "https://api.ustream.tv";
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
                NotifyIsNowLive();
            }

            Updating = false;
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/channels/{1}.json", ApiUri, channelId);
            HttpWebRequest req = BuildUstreamHttpWebRequest(new Uri(apiAddressToQuery));

            JObject resp = await GetApiResponseAsync(req).ConfigureAwait(false);

            if (resp == null)
            {
                IsLive = false;
            }
            else
            {
                if (resp["channel"].HasValues)
                {
                    if (HasUpdatedDisplayName == false)
                    {
                        SetDisplayName(resp);
                    }
                    
                    IsLive = ((string)resp["channel"]["status"]).Equals("live");
                }
            }
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
        
        protected override void NotifyIsNowLive()
        {
            string title = string.Format(CultureInfo.CurrentCulture, "{0} is LIVE on Ustream", DisplayName);
            
            NotificationService.Send(title, GoToStream);
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
