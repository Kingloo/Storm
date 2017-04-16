using System;
using System.Net;
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

            HasStreamlinkSupport = true;
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
            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiCall));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));
            
            bool live = false;
            
            if (json != null)
            {
                if (!HasUpdatedDisplayName)
                {
                    if (json.TryGetValue("token", out JToken token))
                    {
                        DisplayName = (string)json["token"];
                    }
                }

                if (json.TryGetValue("online", out JToken online))
                {
                    live = (bool)json["online"];
                }
            }
            
            IsLive = live;
        }

        protected override HttpWebRequest BuildHttpWebRequest(Uri uri)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            HttpWebRequest req = base.BuildHttpWebRequest(uri);

            req.Accept = "application/json; charset=UTF-8";

            return req;
        }
    }
}
