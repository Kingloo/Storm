using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace Storm.Model
{
    public class Hitbox : StreamBase
    {
        #region Properties
        public override Uri Api => new Uri("https://api.hitbox.tv");

        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Hitbox.ico"));
        public override BitmapImage Icon => _icon;

        public override bool HasStreamlinkSupport => false;
        #endregion

        public Hitbox(Uri accountUri)
            : base(accountUri)
        { }
        
        public override async Task UpdateAsync()
        {
            Updating = true;
            
            bool wasLive = IsLive;

            await DetermineIfLiveAsync();

            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(Hitbox));
            }

            Updating = false;
        }
        
        protected override async Task DetermineIfLiveAsync()
        {
            string apiAddressToQuery = $"{Api.AbsoluteUri}/user/{Name}";

            HttpRequestMessage request = BuildRequest(new Uri(apiAddressToQuery));
            
            bool live = IsLive;

            if (await GetApiResponseAsync(request, true).ConfigureAwait(false) is JObject json)
            {
                if (json.HasValues)
                {
                    live = Convert.ToBoolean((int)json["is_live"]);
                }
            }
            
            IsLive = live;
        }
    }
}
