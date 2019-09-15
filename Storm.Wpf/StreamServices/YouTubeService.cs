using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Storm.Wpf.Common;
using Storm.Wpf.Extensions;
using Storm.Wpf.Streams;

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

            (HttpStatusCode status, string html) = await Web.DownloadStringAsync(uri).ConfigureAwait(false);

            if (status != HttpStatusCode.OK) { return failure; }

            bool isLive = html.Contains(isLiveMarker);
            string displayName = string.Empty;

            using (StringReader sr = new StringReader(html))
            {
                string line = string.Empty;

                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (line.Contains(displayNameStartMarker))
                    {
                        if (line.FindBetween(displayNameStartMarker, displayNameEndMarker).FirstOrDefault() is string name)
                        {
                            displayName = name;

                            break;
                        }
                    }
                }
            }

            return (displayName, isLive);
        }
    }
}
