using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Storm.Wpf.Streams;
using static Storm.Wpf.StreamServices.Helpers;

namespace Storm.Wpf.StreamServices
{
    public class YouTubeService : StreamServiceBase
    {
        private const string isLiveMarker = "yt-badge-live";
        private const string displayNameStartMarker = "<meta property=\"og:title\" content=\"";
        private const string displayNameEndMarker = "\">";

        protected override Uri ApiRoot { get; } = new Uri("https://www.youtube.com/user");
        protected override bool HasYouTubeDlSupport { get; } = true;
        public override Type HandlesStreamType { get; } = typeof(YouTubeStream);

        public YouTubeService() { }

        public override Task UpdateAsync(IEnumerable<StreamBase> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }
            if (!streams.Any()) { return Task.CompletedTask; }

            var updateTasks = new List<Task>(streams.Select(stream => UpdateYouTubeStreamAsync(stream)));

            return Task.WhenAll(updateTasks);
        }

        private async Task UpdateYouTubeStreamAsync(StreamBase stream)
        {
            Uri uri = new Uri($"{ApiRoot}/{stream.AccountName}");

            (string displayName, bool isLive) = await GetYouTubeApiResponseAsync(uri);

            if (!String.IsNullOrWhiteSpace(displayName))
            {
                stream.DisplayName = displayName;
            }

            stream.IsLive = isLive;
        }

        private static async Task<(string, bool)> GetYouTubeApiResponseAsync(Uri uri)
        {
            (string, bool) failure = (string.Empty, false);

            (bool success, string html) = await DownloadStringAsync(uri).ConfigureAwait(false);

            if (!success) { return failure; }

            bool isLive = false;
            string displayName = string.Empty;

            using (StringReader sr = new StringReader(html))
            {
                string line = string.Empty;

                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (line.Contains(displayNameStartMarker))
                    {
                        int idxDisplayNameStart = line.IndexOf(displayNameStartMarker) + displayNameStartMarker.Length;
                        int idxDisplayNameEnd = line.IndexOf(displayNameEndMarker, idxDisplayNameStart);

                        int displayNameLength = idxDisplayNameEnd - idxDisplayNameStart;

                        displayName = line.Substring(idxDisplayNameStart, displayNameLength);
                    }

                    if (line.Contains(isLiveMarker))
                    {
                        isLive = true;
                    }
                }
            }

            return (displayName, isLive);
        }
    }
}
