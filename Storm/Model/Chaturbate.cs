using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Storm.Model
{
    public class Chaturbate : StreamBase
    {
        #region Properties
        private readonly static BitmapImage _icon = new BitmapImage(new Uri("pack://application:,,,/Icons/Chaturbate.ico"));
        public override BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }
        #endregion

        public Chaturbate(Uri accountUri) : base(accountUri)
        {
            _icon.Freeze();
        }

        public override async Task UpdateAsync()
        {
            Updating = true;

            bool wasLive = IsLive;

            await DetermineIfLiveAsync();

            if (wasLive == false && IsLive == true)
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

            if (String.IsNullOrWhiteSpace(response) == false)
            {
                live = !response.Contains("Room is currently offline");
                
                // website must contain NEITHER "Room is currently offline" NOR "banned"
                //IsLive = !(website.Contains("Room is currently offline") && website.Contains("banned"));
            }

            IsLive = live;
        }
    }
}
