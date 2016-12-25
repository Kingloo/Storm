using System;
using System.Net;
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
    }
}


// string apiAddressToQueryForStreams = string.Format("{0}/{1}?source=embed&callback=onUserLoad", apiUri, Name);
// var thing = (string)json["broadcasts"].FirstOrDefault()["streams"]["rtsp"]["url"];