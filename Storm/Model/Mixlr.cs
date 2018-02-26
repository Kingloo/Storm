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
        public override Uri Api => new Uri("https://api.mixlr.com/users");

        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Mixlr.ico"));
        public override BitmapImage Icon => _icon;

        public override bool HasStreamlinkSupport => false;
        #endregion

        public Mixlr(Uri userUrl)
            : base(userUrl)
        { }

        public override async Task UpdateAsync()
        {
            Updating = true;
            
            bool wasLive = IsLive;
            
            await DetermineIfLiveAsync();
            
            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(Mixlr));
            }

            Updating = false;
        }
        
        protected override async Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = $"{Api.AbsoluteUri}/{Name}";

            HttpWebRequest request = BuildHttpWebRequest(new Uri(apiAddressToQuery));
            
            JObject json = (JObject)(await GetApiResponseAsync(request, true)
                .ConfigureAwait(false));

            bool live = IsLive;
            
            if (json != null)
            {
                if (json.HasValues)
                {
                    if (!HasUpdatedDisplayName)
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
            
            HasUpdatedDisplayName = DisplayName.Equals(Name) ? false : true;
        }
    }
}


// string apiAddressToQueryForStreams = string.Format("{0}/{1}?source=embed&callback=onUserLoad", apiUri, Name);
// var thing = (string)json["broadcasts"].FirstOrDefault()["streams"]["rtsp"]["url"];