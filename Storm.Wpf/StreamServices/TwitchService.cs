using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Storm.Wpf.Streams;
using static Storm.Wpf.StreamServices.Helpers;

namespace Storm.Wpf.StreamServices
{
    public class TwitchService : StreamServiceBase
    {
        private const string clientIdHeaderName = "Client-ID";
        private const string clientIdHeaderValue = "ewvlchtxgqq88ru9gmfp1gmyt6h2b93";

        private static readonly ConcurrentDictionary<Int64, string> gameIdCache = new ConcurrentDictionary<Int64, string>();

        protected override Uri ApiRoot { get; } = new Uri("https://api.twitch.tv/helix");
        protected override bool HasStreamlinkSupport { get; } = true;
        protected override bool HasYouTubeDlSupport { get; } = true;

        public override Type HandlesStreamType { get; } = typeof(TwitchStream);


        public TwitchService() { }


        public override async Task UpdateAsync(IEnumerable<StreamBase> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }
            if (!streams.Any()) { return; }

            var results = CreateResultsHolder(streams.Select(s => s.AccountName));

            await GetUserIdAndDisplayNameAsync(results);

            await GetIsLiveAndGameIdAsync(results);

            await UpdateGameNameCache(results);

            SetValues(streams, results);
        }

        private static Dictionary<string, (Int64, string, bool, Int64)> CreateResultsHolder(IEnumerable<string> userNames)
        {
            var results = new Dictionary<string, (Int64, string, bool, Int64)>();

            foreach (string eachAccountName in userNames)
            {
                var defaults = (Int64.MinValue, string.Empty, false, 0);

                results.Add(eachAccountName, defaults);
            }

            return results;
        }

        private async Task GetUserIdAndDisplayNameAsync(Dictionary<string, (Int64, string, bool, Int64)> results)
        {
            string query = BuildUserIdQuery(results.Keys);

            (bool success, JArray data) = await GetTwitchResponseAsync(query).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindAccountName = each.TryGetValue("login", out JToken loginToken);
                bool couldFindUserId = each.TryGetValue("id", out JToken idToken);
                bool couldFindDisplayName = each.TryGetValue("display_name", out JToken displayNameToken);

                if (couldFindAccountName && couldFindUserId && couldFindDisplayName)
                {
                    string accountName = (string)loginToken;
                    Int64 userId = (Int64)idToken;
                    string displayName = (string)displayNameToken;

                    results[accountName] = (userId, displayName, false, 0); // IsLive and GameId come later, so we keep them at default
                }
            }
        }

        private async Task GetIsLiveAndGameIdAsync(Dictionary<string, (Int64, string, bool, Int64)> results)
        {
            var query = BuildStatusQuery(results.Values.Select(kvp => kvp.Item1)); // Item1 is userId

            (bool success, JArray data) = await GetTwitchResponseAsync(query).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindUserId = each.TryGetValue("user_id", out JToken userIdToken);
                bool couldFindType = each.TryGetValue("type", out JToken typeToken);
                bool couldFindGameId = each.TryGetValue("game_id", out JToken gameIdToken);

                if (couldFindUserId && couldFindType && couldFindGameId)
                {
                    Int64 userId = (Int64)userIdToken;
                    bool isLive = (string)typeToken == "live";
                    Int64 gameId = (Int64)gameIdToken;

                    string accountNameKey = results.Keys.SingleOrDefault(key => results[key].Item1 == userId);

                    var newValueForAccountNameKey = (results[accountNameKey].Item1, results[accountNameKey].Item2, isLive, gameId);

                    results[accountNameKey] = newValueForAccountNameKey;
                }
            }
        }

        private async Task UpdateGameNameCache(Dictionary<string, (Int64, string, bool, Int64)> results)
        {
            var gameIds = results
                .Select(kvp => kvp.Value.Item4)
                .Where(id => !gameIdCache.ContainsKey(id));

            if (!gameIds.Any()) { return; }

            string query = BuildGameIdsQuery(gameIds);

            (bool success, JArray data) = await GetTwitchResponseAsync(query).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindId = each.TryGetValue("id", out JToken idToken);
                bool couldFindGameName = each.TryGetValue("name", out JToken gameToken);

                if (couldFindId && couldFindGameName)
                {
                    Int64 gameId = (Int64)idToken;
                    string gameName = (string)gameToken;

                    gameIdCache.AddOrUpdate(gameId, gameName, (i, s) => gameName);
                }
            }
        }

        private static void SetValues(IEnumerable<StreamBase> streams, Dictionary<string, (Int64, string, bool, Int64)> results)
        {
            foreach (TwitchStream stream in streams)
            {
                if (results.TryGetValue(stream.AccountName, out (Int64, string, bool, Int64) data))
                {
                    stream.UserId = data.Item1;
                    stream.DisplayName = data.Item2;
                    
                    stream.Game = gameIdCache.ContainsKey(data.Item4)
                        ? gameIdCache[data.Item4]
                        : string.Empty;

                    stream.IsLive = data.Item3; // MUST set .Game before .IsLive otherwise notification will fire without game name
                }
            }
        }


        private string BuildUserIdQuery(IEnumerable<string> userNames)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/users?");

            foreach (string userName in userNames)
            {
                query.Append($"login={userName}&");
            }

            return query.ToString();
        }

        private string BuildStatusQuery(IEnumerable<Int64> userIds)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/streams?");

            foreach (Int64 userId in userIds)
            {
                query.Append($"user_id={userId}&");
            }

            return query.ToString();
        }

        private string BuildGameIdsQuery(IEnumerable<Int64> gameIds)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/games?");

            foreach (Int64 id in gameIds)
            {
                query.Append($"id={id}&");
            }

            return query.ToString();
        }


        private static async Task<(bool, JArray)> GetTwitchResponseAsync(string query)
        {
            (bool, JArray) failure = (false, null);

            if (!Uri.TryCreate(query, UriKind.Absolute, out Uri uri)) { return failure; }

            Action<HttpRequestMessage> configureHeaders = request => request.Headers.Add(clientIdHeaderName, clientIdHeaderValue);

            (bool success, string rawJson) = await DownloadStringAsync(uri, configureHeaders).ConfigureAwait(false);

            if (!success) { return failure; }
            if (!TryParseJson(rawJson, out JObject json)) { return failure; }
            if (!json.TryGetValue("data", out JToken dataToken)) { return failure; }
            if (!(dataToken is JArray data)) { return failure; }
            //if (!data.HasValues) { return failure; }

            return (true, data);
        }
    }
}
