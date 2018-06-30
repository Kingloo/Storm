using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Mixer : StreamBase
    {
        #region Properties
        public override Uri Api => new Uri("https://mixer.com/api/v1");

        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Mixer.ico"));
        public override BitmapImage Icon => _icon;

        public override bool HasStreamlinkSupport => true;
        #endregion

        public Mixer(Uri accountUri)
            : base(accountUri)
        { }
        
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
            string apiCall = $"{Api.AbsoluteUri}/channels/{Name}";

            HttpRequestMessage request = BuildRequest(new Uri(apiCall));

            bool live = IsLive;

            if (await GetApiResponseAsync(request, true).ConfigureAwait(false) is JObject json)
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

        protected override HttpRequestMessage BuildRequest(Uri uri)
        {
            HttpRequestMessage request = base.BuildRequest(uri);

            request.Headers.Add("Accept", "application/json; charset=UTF-8");

            return request;
        }
    }
}
