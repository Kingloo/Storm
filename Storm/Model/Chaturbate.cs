using System;
using System.Net;
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
            HttpWebRequest request = BuildHttpWebRequest(Uri);

            string response = (string)(await GetApiResponseAsync(request, false).ConfigureAwait(false));

            bool live = false;

            if (!String.IsNullOrWhiteSpace(response))
            {
                live = !response.Contains("Room is currently offline")
                    && !response.Contains("meta name=\"keywords\" content=\"Login, Chaturbate login\"")
                    && !response.Contains("has been banned");
                
                // website must contain NEITHER "Room is currently offline" NOR "banned"
                //IsLive = !(website.Contains("Room is currently offline") && website.Contains("banned"));
            }

            IsLive = live;
        }
    }
}
