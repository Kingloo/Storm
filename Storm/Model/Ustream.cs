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
                await DetermineChannelIdAsync();
            }
            
            bool wasLive = IsLive;

            await DetermineIfLiveAsync();
            
            if (wasLive == false && IsLive == true)
            {
                NotifyIsNowLive(nameof(Ustream));
            }

            Updating = false;
        }

        private async Task DetermineChannelIdAsync()
        {
            HttpWebRequest req = BuildHttpWebRequest(Uri);

            string response = (string)(await GetApiResponseAsync(req, false).ConfigureAwait(false));

            if (String.IsNullOrWhiteSpace(response) == false)
            {
                string beginning = "\"channelId\":";
                string ending = ",";

                IReadOnlyList<string> results = response.FindBetween(beginning, ending);

                if (results.Count > 0)
                {
                    channelId = results.First();
                }
            }
        }

        protected async override Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = string.Format("{0}/channels/{1}.json", ApiUri, channelId);
            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));
            
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
                    
                    live = ((string)json["channel"]["status"])
                        .Equals("live");
                }
            }
            
            IsLive = live;
        }
        
        private void TrySetDisplayName(JObject resp)
        {
            string displayName = (string)resp["channel"]["title"];

            if (String.IsNullOrEmpty(displayName) == false)
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
