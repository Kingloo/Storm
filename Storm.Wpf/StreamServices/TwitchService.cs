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
        private static readonly ConcurrentDictionary<Int64, string> gameIdCache = new ConcurrentDictionary<Int64, string>();

        protected override Uri ApiRoot { get; } = new Uri("https://api.twitch.tv/helix");
        public override Type HandlesStreamType { get; } = typeof(TwitchStream);

        public TwitchService() { }

        public override async Task UpdateAsync(IEnumerable<StreamBase> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }
            if (!streams.Any()) { return; }

            IEnumerable<string> userNames = streams
                .Select(stream => stream.AccountName)
                .ToList();

            //          key    Val.It1 Val.It2
            //         userId  accName DisName
            Dictionary<Int64, (string, string)> userIdAccountNameAndDisplayName = await GetUserIdAndDisplayNameAsync(userNames);

            foreach (TwitchStream each in streams)
            {
                var userId = userIdAccountNameAndDisplayName.Single(id => id.Value.Item1 == each.AccountName);

                each.UserId         =   userId.Key;
                each.DisplayName    =   userId.Value.Item2;
            }

            //         key   Val.It1 Val.It2
            //        userId  isLive gameId
            Dictionary<Int64, (bool, Int64)> userIdIsLiveAndGameId = await GetStatusesAsync(userIdAccountNameAndDisplayName.Keys);

            await AddOrUpdateGameNames(
                userIdIsLiveAndGameId
                .Select(kvp => kvp.Value.Item2)
                .ToList()
                );

            ProcessTwitchResponses(streams, userIdIsLiveAndGameId);
        }


        private async Task<Dictionary<Int64, (string, string)>> GetUserIdAndDisplayNameAsync(IEnumerable<string> userNames)
        {
            string query = BuildUserIdQuery(userNames);

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            var userIdAccountNameAndDisplayName = new Dictionary<Int64, (string, string)>();

            if (!success) { return userIdAccountNameAndDisplayName; }

            foreach (JObject each in data)
            {
                bool couldFindUserId = each.TryGetValue("id", out JToken idToken);
                bool couldFindAccountName = each.TryGetValue("login", out JToken loginToken);
                bool couldFindDisplayName = each.TryGetValue("display_name", out JToken displayNameToken);

                if (couldFindUserId && couldFindAccountName && couldFindDisplayName)
                {
                    Int64 userId = (Int64)idToken;
                    string accountName = (string)loginToken;
                    string displayName = (string)displayNameToken;

                    userIdAccountNameAndDisplayName.Add(userId, (accountName, displayName));
                }
            }

            return userIdAccountNameAndDisplayName;
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


        private async Task<Dictionary<Int64, (bool, Int64)>> GetStatusesAsync(IEnumerable<Int64> userIds)
        {
            var query = BuildStatusQuery(userIds);

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            var results = new Dictionary<Int64, (bool, Int64)>();

            if (!success) { return results; }

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

                    results.Add(userId, (isLive, gameId));
                }
            }

            return results;
        }

        private string BuildStatusQuery(IEnumerable<Int64> userNames)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/streams?");

            foreach (Int64 userId in userNames)
            {
                query.Append($"user_id={userId}&");
            }

            return query.ToString();
        }


        private async Task AddOrUpdateGameNames(IEnumerable<Int64> gameIds)
        {
            string query = BuildGameIdsQuery(gameIds);

            (bool success, JArray data) = await GetTwitchResponseAsync(query).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindId = each.TryGetValue("id", out JToken idToken);
                bool couldFindName = each.TryGetValue("name", out JToken gameToken);

                if (couldFindId && couldFindName)
                {
                    Int64 gameId = (Int64)idToken;
                    string gameName = (string)gameToken;

                    gameIdCache.AddOrUpdate(gameId, gameName, (i, s) => gameName);
                }
            }
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


        private static void ProcessTwitchResponses(IEnumerable<StreamBase> streams, Dictionary<Int64, (bool, Int64)> values)
        {
            foreach (TwitchStream stream in streams)
            {
                if (values.TryGetValue(stream.UserId, out (bool, Int64) value))
                {
                    (bool isLive, Int64 gameId) = value;

                    stream.IsLive = isLive;
                    stream.Game = gameIdCache[gameId];
                }
            }
        }


        private static async Task<(bool, JArray)> GetTwitchResponseAsync(string query)
        {
            (bool, JArray) failure = (false, null);

            if (!Uri.TryCreate(query, UriKind.Absolute, out Uri uri)) { return failure; }

            Action<HttpRequestMessage> configureHeaders = request => request.Headers.Add("Client-ID", "ewvlchtxgqq88ru9gmfp1gmyt6h2b93");

            (bool success, string rawJson) = await DownloadStringAsync(uri, configureHeaders).ConfigureAwait(false);

            if (!success) { return failure; }
            if (!TryParseJson(rawJson, out JObject json)) { return failure; }
            if (!json.TryGetValue("data", out JToken dataToken)) { return failure; }
            if (!(dataToken is JArray data)) { return failure; }
            if (!data.HasValues) { return failure; }

            return (true, data);
        }
    }
}
