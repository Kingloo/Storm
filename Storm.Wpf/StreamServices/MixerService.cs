using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Storm.Wpf.Streams;
using static Storm.Wpf.StreamServices.Helpers;

namespace Storm.Wpf.StreamServices
{
    public class MixerService : StreamServiceBase
    {
        protected override Uri ApiRoot { get; } = new Uri("https://mixer.com/api/v1");
        public override Type HandlesStreamType { get; } = typeof(MixerStream);

        public MixerService() { }

        public override Task UpdateAsync(IEnumerable<StreamBase> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }
            if (!streams.Any()) { return Task.CompletedTask; }

            List<Task> updateTasks = streams
                .Select(stream => UpdateMixerStreamAsync(stream))
                .ToList();

            return Task.WhenAll(updateTasks);
        }

        private async Task UpdateMixerStreamAsync(StreamBase stream)
        {
            Uri uri = new Uri($"{ApiRoot}/channels/{stream.AccountName}");

            (string userName, bool isLive) = await GetMixerApiResponseAsync(uri);

            if (!String.IsNullOrWhiteSpace(userName))
            {
                stream.DisplayName = userName;
            }

            stream.IsLive = isLive;
        }

        private static async Task<(string, bool)> GetMixerApiResponseAsync(Uri uri)
        {
            (string, bool) failure = (string.Empty, false);

            (bool success, string rawJson) = await DownloadStringAsync(uri).ConfigureAwait(false);

            if (!success) { return failure; }
            if (TryParseJson(rawJson, out JObject json)) { return failure; }
            if (!json.HasValues) { return failure; }

            if (json.TryGetValue("token", out JToken userNameToken)
                && json.TryGetValue("online", out JToken onlineToken))
            {
                string displayName = (string)userNameToken;
                bool isLive = (bool)onlineToken;

                return (displayName, isLive);
            }

            return failure;
        }
    }
}
