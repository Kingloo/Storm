using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Storm.Wpf.Streams;
using static Storm.Wpf.StreamServices.Helpers;

namespace Storm.Wpf.StreamServices
{
    public class MixlrService
    {
        private static readonly Uri apiRoot = new Uri("https://api.mixlr.com/users");

        public static Task UpdateAsync(IEnumerable<MixlrStream> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }
            if (!streams.Any()) { return Task.CompletedTask; }

            var updateTasks = new List<Task>(streams.Select(stream => UpdateMixlrStreamAsync(stream)));

            return Task.WhenAll(updateTasks);
        }

        private static async Task UpdateMixlrStreamAsync(MixlrStream stream)
        {
            Uri uri = new Uri($"{apiRoot}/{stream.AccountName}");

            (string displayName, bool isLive) = await GetMixlrApiResponseAsync(uri);

            if (!String.IsNullOrEmpty(displayName))
            {
                stream.DisplayName = displayName;
            }

            stream.IsLive = isLive;
        }

        private static async Task<(string, bool)> GetMixlrApiResponseAsync(Uri uri)
        {
            (string, bool) failure = (string.Empty, false);

            (bool success, string text) = await DownloadStringAsync(uri).ConfigureAwait(false);

            if (!success) { return failure; }
            if (!TryParseJson(text, out JObject json)) { return failure; }
            if (!json.HasValues) { return failure; }

            if (json.TryGetValue("username", out JToken usernameToken)
                && json.TryGetValue("is_live", out JToken isLiveToken))
            {
                string username = (string)usernameToken;
                bool isLive = (bool)isLiveToken;

                return (username, isLive);
            }

            return failure;
        }
    }
}
