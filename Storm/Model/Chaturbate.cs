using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Storm.Model
{
    public class Chaturbate : StreamBase
    {
        #region Properties
        public override Uri Api => null;

        private readonly static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/Chaturbate.ico"));
        public override BitmapImage Icon => _icon;

        public override bool HasStreamlinkSupport => true;
        #endregion

        public Chaturbate(Uri accountUri)
            : base(accountUri)
        { }

        public override async Task UpdateAsync()
        {
            Updating = true;

            bool wasLive = IsLive;

            await DetermineIfLiveAsync();

            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(Chaturbate));
            }

            Updating = false;
        }

        protected override async Task DetermineIfLiveAsync()
        {
            HttpRequestMessage request = BuildRequest(Uri);

            string response = (string)(await GetApiResponseAsync(request, false).ConfigureAwait(false));

            bool live = false;

            if (!String.IsNullOrWhiteSpace(response))
            {
                // website must contain NEITHER "Room is currently offline" NOR "has been banned" NOR be a login page
                live = !response.Contains("Room is currently offline")
                    && !response.Contains("meta name=\"keywords\" content=\"Login, Chaturbate login\"")
                    && !response.Contains("has been banned");
                
                
                //IsLive = !(website.Contains("Room is currently offline") && website.Contains("banned"));
            }

            IsLive = live;
        }
    }
}
