// curl -X GET -H "Client-ID: ewvlchtxgqq88ru9gmfp1gmyt6h2b93" "https://api.twitch.tv/helix/streams?user_id=18074328&user_id=188854137"

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Storm.Wpf.Common;
using Storm.Wpf.Streams;

namespace Storm.Wpf.StreamServices
{
    public class TwitchServiceResponse
    {
        public string UserName { get; }

        public Int64 UserId { get; set; } = Int64.MinValue;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsLive { get; set; } = false;
        public Int64 GameId { get; set; } = 0L;

        public TwitchServiceResponse(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException($"{nameof(userName)} was .IsNullOrWhiteSpace", nameof(userName));
            }

            UserName = userName;
        }
    }

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

            var holder = CreateResultsHolder(streams.Select(s => s.AccountName));

            await GetUserIdAndDisplayNameAsync(holder);

            await GetIsLiveAndGameIdAsync(holder);

            await UpdateGameNameCache(holder);

            SetValues(streams, holder);
        }

        /// <summary>
        /// Creates a holder for the results from the Twitch API, that will get filled out by the UpdateAsync methods, then applied to the streams collection.
        /// </summary>
        /// <param name="userNames">The Twitch user names we would like to update.</param>
        /// <returns>Key is Twitch username, Int64 is user id, string is display name, bool is IsLive, Int64 is game id.</returns>
        private static IReadOnlyList<TwitchServiceResponse> CreateResultsHolder(IEnumerable<string> userNames)
        {
            var holder = new List<TwitchServiceResponse>();

            foreach (string userName in userNames)
            {
                var response = new TwitchServiceResponse(userName);

                holder.Add(response);
            }

            return holder;
        }

        private async Task GetUserIdAndDisplayNameAsync(IReadOnlyList<TwitchServiceResponse> holder)
        {
            string query = BuildUserIdQuery(holder.Select(each => each.UserName));

            (bool success, JArray data) = await GetTwitchResponseAsync(query).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindAccountName = each.TryGetValue("login", out JToken loginToken);
                bool couldFindUserId = each.TryGetValue("id", out JToken idToken);
                bool couldFindDisplayName = each.TryGetValue("display_name", out JToken displayNameToken);

                if (couldFindAccountName)
                {
                    string accountName = (string)loginToken;

                    var response = holder.Single(resp => resp.UserName == accountName);

                    if (couldFindDisplayName)
                    {
                        response.DisplayName = (string)displayNameToken;
                    }

                    if (couldFindUserId)
                    {
                        response.UserId = (Int64)idToken;
                    }
                }
            }
        }

        private async Task GetIsLiveAndGameIdAsync(IReadOnlyList<TwitchServiceResponse> holder)
        {
            string query = BuildStatusQuery(holder.Select(each => each.UserId));

            (bool success, JArray data) = await GetTwitchResponseAsync(query).ConfigureAwait(false);

            if (!success) { return; }

            foreach (JObject each in data)
            {
                bool couldFindUserId = each.TryGetValue("user_id", out JToken userIdToken);
                bool couldFindType = each.TryGetValue("type", out JToken typeToken);
                bool couldFindGameId = each.TryGetValue("game_id", out JToken gameIdToken);

                if (couldFindUserId)
                {
                    Int64 userId = (Int64)userIdToken;

                    var response = holder.Single(resp => resp.UserId == userId);

                    if (couldFindType)
                    {
                        response.IsLive = (string)typeToken == "live";
                    }

                    if (couldFindGameId)
                    {
                        // game_id can be present in the json, but a have blank value e.g. "game_id": "",
                        // under this circumstance, using the Json.Net cast ("(int)gameIdToken") throws a FormatException
                        // hence the extra checking

                        string gameIdString = (string)gameIdToken;

                        if (!String.IsNullOrEmpty(gameIdString))
                        {
                            if (Int64.TryParse(gameIdString, out Int64 gameId))
                            {
                                response.GameId = gameId;
                            }
                        }
                    }
                }
            }
        }

        private async Task UpdateGameNameCache(IReadOnlyList<TwitchServiceResponse> holder)
        {
            var unknownGameIds = holder
                .Select(each => each.GameId)
                .Where(id => id != 0) // game id is 0 (zero) when they are offline
                .Where(id => !gameIdCache.ContainsKey(id));

            if (!unknownGameIds.Any()) { return; }

            string query = BuildGameIdsQuery(unknownGameIds);

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

        private static void SetValues(IEnumerable<StreamBase> streams, IReadOnlyList<TwitchServiceResponse> holder)
        {
            foreach (TwitchStream stream in streams)
            {
                var response = holder.Single(each => each.UserName == stream.AccountName);

                stream.UserId = response.UserId;

                if (!String.IsNullOrWhiteSpace(response.DisplayName))
                {
                    stream.DisplayName = response.DisplayName;
                }
                
                // MUST set .Game before .IsLive otherwise notification will fire without game name
                // the notification would read "Fred is LIVE" instead of "Fred is LIVE and playing Sqoon"

                if (gameIdCache.TryGetValue(response.GameId, out string gameName))
                {
                    stream.Game = gameName;
                }

                stream.IsLive = response.IsLive;
            }
        }


        private string BuildUserIdQuery(IEnumerable<string> userNames)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/users?");

            foreach (string userName in userNames)
            {
                query.Append($"&login={userName}");
            }

            return query.ToString();
        }

        private string BuildStatusQuery(IEnumerable<Int64> userIds)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/streams?");

            foreach (Int64 userId in userIds)
            {
                query.Append($"&user_id={userId}");
            }

            return query.ToString();
        }

        private string BuildGameIdsQuery(IEnumerable<Int64> gameIds)
        {
            StringBuilder query = new StringBuilder($"{ApiRoot.AbsoluteUri}/games?");

            foreach (Int64 id in gameIds)
            {
                query.Append($"&id={id}");
            }

            return query.ToString();
        }


        private static async Task<(bool, JArray)> GetTwitchResponseAsync(string query)
        {
            (bool, JArray) failure = (false, null);

            if (!Uri.TryCreate(query, UriKind.Absolute, out Uri uri)) { return failure; }

            Action<HttpRequestMessage> configureHeaders = request => request.Headers.Add(clientIdHeaderName, clientIdHeaderValue);

            (HttpStatusCode status, string rawJson) = await Web.DownloadStringAsync(uri, configureHeaders).ConfigureAwait(false);

            if (status != HttpStatusCode.OK)
            {
                if (status.ToString() == "429") // rate limiting, HttpStatusCode enum does not contain a 429 option
                {
                    string message = $"HTTP 429 (rate limit) on request to {query}";

                    await Log.MessageAsync(message).ConfigureAwait(false);
                }

                return failure;
            }

            if (!Json.TryParse(rawJson, out JObject json)) { return failure; }
            if (!json.TryGetValue("data", out JToken dataToken)) { return failure; }
            if (!(dataToken is JArray data)) { return failure; }

            return (true, data);
        }
    }
}
