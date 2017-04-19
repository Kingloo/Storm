using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Storm.Extensions;

namespace Storm.Model
{
    public class YouTube : StreamBase
    {
        #region Properties
        private static BitmapImage _icon
            = new BitmapImage(new Uri("pack://application:,,,/Icons/YouTube.ico"));
        public override BitmapImage Icon => _icon;
        #endregion

        public YouTube(Uri uri)
            : base(uri) { }

        protected override string SetAccountName(Uri uri)
        {
            if (uri == null) { throw new ArgumentNullException(nameof(uri)); }

            string newName = uri.AbsoluteUri;
            
            // 0: /
            // 1: moetv1337
            
            // 0: /
            // 1: user/
            // 2: moetv1337
            
            // 0: /
            // 1: channel/
            // 2: UCayBk2oaTaiUw1Z4r_VKuAg

            if (uri.Segments.Length == 2)
            {
                newName = uri.Segments[1];
            }
            else if (uri.Segments.Length > 2)
            {
                char slash = Char.Parse("/");

                newName = uri
                    .Segments[2]
                    .TrimEnd(slash);
            }

            return newName;
        }

        public override async Task UpdateAsync()
        {
            Updating = true;

            bool wasLive = IsLive;

            if (!HasUpdatedDisplayName)
            {
                await TrySetDisplayNameAsync();
            }

            await DetermineIfLiveAsync();

            if (!wasLive && IsLive)
            {
                NotifyIsNowLive(nameof(YouTube));
            }

            Updating = false;
        }

        private async Task TrySetDisplayNameAsync()
        {
            HttpWebRequest request = BuildHttpWebRequest(Uri);

            string response = (string)(await GetApiResponseAsync(request, false)
                .ConfigureAwait(false));

            if (!String.IsNullOrEmpty(response))
            {
                string beginning = "<meta property=\"og:title\" content=\"";
                string ending = "\">";

                IReadOnlyList<string> results = response.FindBetween(beginning, ending);

                if (results.Any())
                {
                    DisplayName = results.First();

                    HasUpdatedDisplayName = true;
                }
            }
        }

        protected override async Task DetermineIfLiveAsync()
        {
            HttpWebRequest request = BuildHttpWebRequest(Uri);

            string response = (string)(await GetApiResponseAsync(request, false)
                .ConfigureAwait(false));

            bool live = false;

            if (!String.IsNullOrEmpty(response))
            {
                live = response.Contains("yt-badge-live");
            }

            IsLive = live;
        }
    }
}
