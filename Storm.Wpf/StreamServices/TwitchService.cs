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
    public static class TwitchService
    {
        private static readonly Uri apiRoot = new Uri("https://api.twitch.tv/helix");
        private static readonly ConcurrentDictionary<Int64, string> gameIdCache = new ConcurrentDictionary<Int64, string>();

        public static async Task UpdateAsync(IEnumerable<TwitchStream> streams)
        {
            if (streams is null) { throw new ArgumentNullException(nameof(streams)); }

            IEnumerable<string> userNames = streams
                .Select(stream => stream.AccountName)
                .ToList();

            Dictionary<string, string> userNameDisplayName = await GetDisplayNamesAsync(userNames);
            Dictionary<string, (bool, Int64)> userNameStatusAndGameId = await GetStatusesAsync(userNames);

            var gameIds = userNameStatusAndGameId
                .Select(kvp => kvp.Value.Item2)
                .ToList();

            await AddOrUpdateGameNames(gameIds);

            ProcessTwitchResponses(streams, userNameDisplayName, userNameStatusAndGameId);
        }


        private static async Task<Dictionary<string, string>> GetDisplayNamesAsync(IEnumerable<string> userNames)
        {
            var userNameDisplayNamePairs = new Dictionary<string, string>();

            string query = BuildDisplayNamesQuery(userNames);

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            if (!success) { return null; }

            foreach (JObject each in data)
            {
                bool couldFindUserName = each.TryGetValue("login", out JToken loginToken);
                bool couldFindDisplayName = each.TryGetValue("display_name", out JToken displayNameToken);

                if (couldFindUserName && couldFindDisplayName)
                {
                    string userName = (string)loginToken;
                    string displayName = (string)displayNameToken;

                    userNameDisplayNamePairs.Add(userName, displayName);
                }
            }

            return userNameDisplayNamePairs;
        }

        private static string BuildDisplayNamesQuery(IEnumerable<string> userNames)
        {
            StringBuilder query = new StringBuilder($"{apiRoot.AbsoluteUri}/users?");

            foreach (string userName in userNames)
            {
                query.Append($"login={userName}&");
            }

            return query.ToString();
        }


        private static async Task<Dictionary<string, (bool, Int64)>> GetStatusesAsync(IEnumerable<string> userNames)
        {
            var query = BuildStatusQuery(userNames);

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            if (!success) { return null; }

            var results = new Dictionary<string, (bool, long)>();

            foreach (JObject each in data)
            {
                bool couldFindUserName = each.TryGetValue("user_name", out JToken userNameToken);
                bool couldFindType = each.TryGetValue("type", out JToken typeToken);
                bool couldFindGameId = each.TryGetValue("game_id", out JToken gameIdToken);

                if (couldFindUserName && couldFindType && couldFindGameId)
                {
                    string userName = (string)userNameToken;
                    bool isLive = (string)typeToken == "live";
                    Int64 gameId = (Int64)gameIdToken;

                    results.Add(userName, (isLive, gameId));
                }
            }

            return results;
        }

        private static string BuildStatusQuery(IEnumerable<string> userNames)
        {
            StringBuilder query = new StringBuilder($"{apiRoot.AbsoluteUri}/streams?");

            foreach (string userName in userNames)
            {
                query.Append($"user_login={userName}&");
            }

            return query.ToString();
        }


        private static async Task AddOrUpdateGameNames(IEnumerable<Int64> gameIds)
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

        private static string BuildGameIdsQuery(IEnumerable<Int64> gameIds)
        {
            StringBuilder query = new StringBuilder($"{apiRoot.AbsoluteUri}/games?");

            foreach (Int64 id in gameIds)
            {
                query.Append($"id={id}&");
            }

            return query.ToString();
        }


        private static void ProcessTwitchResponses(IEnumerable<TwitchStream> streams, Dictionary<string, string> userNameDisplayName, Dictionary<string, (bool, Int64)> userNameStatusAndGameId)
        {
            foreach (TwitchStream stream in streams)
            {
                if (userNameDisplayName.TryGetValue(stream.AccountName, out string displayName))
                {
                    stream.DisplayName = displayName;
                }

                if (userNameStatusAndGameId.TryGetValue(stream.AccountName, out (bool isLive, Int64 gameId) res))
                {
                    stream.IsLive = res.isLive;

                    if (stream.IsLive)
                    {
                        stream.Game = gameIdCache[res.gameId];
                    }
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
