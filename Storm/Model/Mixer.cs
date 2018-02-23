using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Mixer : StreamBase
    {
        #region Properties
        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Mixer.ico"));
        public override BitmapImage Icon => _icon;
        #endregion

        public Mixer(Uri accountUri)
            : base(accountUri)
        {
            ApiUri = "https://mixer.com/api/v1";

            HasStreamlinkSupport = true;
        }
        
        public override async Task UpdateAsync()
        {
            Updating = true;

            bool wasLive = IsLive;

            await DetermineIfLiveAsync();

            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(Mixer));
            }

            Updating = false;
        }

        protected override async Task DetermineIfLiveAsync()
        {
            string apiCall = $"{ApiUri}/channels/{Name}";

            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiCall));

            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = IsLive;

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
