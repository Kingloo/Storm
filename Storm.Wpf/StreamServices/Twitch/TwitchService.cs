using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static Storm.Wpf.StreamServices.Helpers;

namespace Storm.Wpf.StreamServices.Twitch
{
    public static class TwitchService
    {
        private static readonly Uri apiRoot = new Uri("https://api.twitch.tv/helix");

        public static ConcurrentDictionary<Int64, string> GameIdCache { get; } = new ConcurrentDictionary<Int64, string>();


        public static async Task<TwitchServiceResponse> UpdateAsync(TwitchServiceRequest request)
        {
            if (request is null) { throw new ArgumentNullException(nameof(request)); }

            var response = new TwitchServiceResponse();

            await UpdateDisplayNamesAsync(request, response).ConfigureAwait(false);

            await UpdateStatusAsync(request, response).ConfigureAwait(false);

            await UpdateGameIdsAsync().ConfigureAwait(false);

            return response;
        }


        private static async Task UpdateDisplayNamesAsync(TwitchServiceRequest request, TwitchServiceResponse response)
        {
            StringBuilder query = new StringBuilder($"{apiRoot.AbsoluteUri}/users?");

            foreach (string userName in request.UserNames)
            {
                query.Append($"login={userName}&");
            }

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindUserName      =   each.TryGetValue("login", out JToken loginToken);
                bool couldFindDisplayName   =   each.TryGetValue("display_name", out JToken displayNameToken);

                if (couldFindUserName && couldFindDisplayName)
                {
                    string userName = (string)loginToken;
                    string displayName = (string)displayNameToken;

                    response.DisplayNames.Add(userName, displayName);
                }
            }
        }

        private static async Task UpdateStatusAsync(TwitchServiceRequest request, TwitchServiceResponse response)
        {
            StringBuilder query = new StringBuilder($"{apiRoot}/streams?");

            foreach (string userName in request.UserNames)
            {
                query.Append($"user_login={userName}&");
            }

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindUserName  =   each.TryGetValue("user_name", out JToken userNameToken);
                bool couldFindType      =   each.TryGetValue("type", out JToken typeToken);
                bool couldFindGameId    =   each.TryGetValue("game_id", out JToken gameIdToken);

                if (couldFindUserName && couldFindType && couldFindGameId)
                {
                    string userName = (string)userNameToken;
                    bool isLive = (string)typeToken == "live";
                    Int64 gameId = (Int64)gameIdToken;

                    response.UserNamesThatAreLive.Add(userName);

                    GameIdCache.AddOrUpdate(gameId, string.Empty, (i, old) => old);
                    // if the key already exists, just keep the old string (aka game name)
                }
            }
        }

        private static async Task UpdateGameIdsAsync()
        {
            IEnumerable<Int64> unsetGameIds = GameIdCache
                .Where(kvp => kvp.Value == string.Empty)
                .Select(kvp => kvp.Key)
                .ToList();

            StringBuilder query = new StringBuilder($"{apiRoot}/games?");

            foreach (Int64 id in unsetGameIds)
            {
                query.Append($"id={id}&");
            }

            (bool success, JArray data) = await GetTwitchResponseAsync(query.ToString()).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindId = each.TryGetValue("id", out JToken idToken);
                bool couldFindGameName = each.TryGetValue("name", out JToken gameToken);

                if (couldFindId && couldFindGameName)
                {
                    Int64 gameId = (Int64)idToken;
                    string gameName = (string)gameToken;

                    GameIdCache.AddOrUpdate(gameId, gameName, (i, old) => gameName);
                    // whether the key exists or not, choose the new game name
                }
            }
        }


        private static async Task<(bool, JArray)> GetTwitchResponseAsync(string query)
        {
            (bool, JArray) failure = (false, null);

            if (!Uri.TryCreate(query, UriKind.Absolute, out Uri uri)) { return failure; }

            Action<HttpRequestMessage> configureHeaders = request => request.Headers.Add("Client-ID", "ewvlchtxgqq88ru9gmfp1gmyt6h2b93");

            (bool success, string json) = await DownloadStringAsync(uri, configureHeaders).ConfigureAwait(false);

            if (!success) { return failure; }
            if (!(ParseJson(json) is JObject results)) { return failure; }
            if (!results.TryGetValue("data", out JToken dataToken)) { return failure; }
            if (!(dataToken is JArray data)) { return failure; }
            if (!data.HasValues) { return failure; }

            return (true, data);
        }
    }
}
