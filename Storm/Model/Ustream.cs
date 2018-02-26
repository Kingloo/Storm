using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public override Uri Api => new Uri("https://api.ustream.tv");

        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Ustream.ico"));
        public override BitmapImage Icon => _icon;

        public override bool HasStreamlinkSupport => true;
        #endregion

        public Ustream(Uri uri)
            : base(uri)
        { }

        public async override Task UpdateAsync()
        {
            Updating = true;

            if (String.IsNullOrWhiteSpace(channelId))
            {
                await DetermineChannelIdAsync();
            }
            
            bool wasLive = IsLive;

            await DetermineIfLiveAsync();
            
            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(Ustream));
            }

            Updating = false;
        }

        private async Task DetermineChannelIdAsync()
        {
            HttpWebRequest req = BuildHttpWebRequest(Uri);

            string response = (string)(await GetApiResponseAsync(req, false)
                .ConfigureAwait(false));

            if (!String.IsNullOrWhiteSpace(response))
            {
                string beginning = "\"channelId\":";
                string ending = ",";

                IReadOnlyList<string> results = response.FindBetween(beginning, ending);

                if (results.Any())
                {
                    channelId = results.First();
                }
            }
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = $"{Api.AbsoluteUri}/channels/{channelId}.json";

            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true).ConfigureAwait(false));

            bool live = IsLive;

            if (json != null)
            {
                if (json.HasValues)
                {
                    if (!HasUpdatedDisplayName)
                    {
                        TrySetDisplayName(json);
                    }
                    
                    live = ((string)json["channel"]["status"]).Equals("live");
                }
            }
            
            IsLive = live;
        }
        
        private void TrySetDisplayName(JObject resp)
        {
            string displayName = (string)resp["channel"]["title"];

            if (!String.IsNullOrEmpty(displayName))
            {
                DisplayName = displayName;

                HasUpdatedDisplayName = true;
            }
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
