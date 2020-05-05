using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StormLib.Common;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
    public class TwitchService : IService, IDisposable
    {
        //417752,    // "Talk Shows & Podcasts"
        //509658,    // "Just Chatting"
        //509670,    // "Science & Technology"

        private static readonly Collection<Int64> unwantedIds = new Collection<Int64>
        {
            26936,     // "Music & Performing Arts"
            509660,    // "Art"
            509481     // "Twitch Sings"
        };

        private static readonly IDictionary<string, string> graphQlRequestHeaders = new Dictionary<string, string>
        {
            { "Host", "gql.twitch.tv" },
            { "User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:68.0) Gecko/20100101 Firefox/75.0" },
            { "Accept", "*/*" },
            { "Accept-Language", "en-GB" },
            { "Accept-Encoding", "gzip, deflate, br" },
            { "Client-Id", "kimne78kx3ncx6brgo4mv6wki5h1ko" }, // this has yet to fail
            { "Origin", "https://www.twitch.tv" },
            { "DNT", "1" },
            //{ "Connection", "keep-alive" },
            { "Upgrade-Insecure-Requests", "1" },
            { "Pragma", "no-cache" },
            { "Cache-Control", "no-cache" }
        };

        private static readonly Uri graphQlEndpoint = new Uri("https://gql.twitch.tv/gql");

        private readonly IDownload download;

        public Type HandlesStreamType { get; } = typeof(TwitchStream);
        
        public TwitchService(IDownload download)
        {
            this.download = download;
        }

        public Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
            => UpdateAsync(new List<IStream> { stream }, preserveSynchronizationContext);

        public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
        {
            if (!streams.Any()) { return Result.NothingToDo; }

            (HttpStatusCode status, string text) = await RequestGraphQlDataAsync(streams).ConfigureAwait(preserveSynchronizationContext);

            if (status != HttpStatusCode.OK)
            {
                return Result.WebFailure;
            }

            if (!Json.TryParse("{\"dummy\":" + text + "}", out JObject? json))
            {
                return Result.ParsingJsonFailed;
            }

#nullable disable
            try
            {
                ParseJson(streams, (JArray)json["dummy"]);

                return Result.Success;
            }
            catch (NullReferenceException)
            {
                return Result.Failure;
            }
#nullable enable
        }

        private Task<(HttpStatusCode, string)> RequestGraphQlDataAsync(IEnumerable<IStream> streams)
        {
            string requestBody = BuildRequestBody(streams);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, graphQlEndpoint)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "text/plain")
            };

            foreach (KeyValuePair<string, string> kvp in graphQlRequestHeaders)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }

            return download.StringAsync(request);
        }

        private static string BuildRequestBody(IEnumerable<IStream> streams)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");

            foreach (IStream stream in streams)
            {
                string beginning = "{\"extensions\":{\"persistedQuery\":{\"sha256Hash\":\"ce18f2832d12cabcfee42f0c72001dfa1a5ed4a84931ead7b526245994810284\",\"version\":1}},\"operationName\":\"ChannelRoot_Channel\",\"variables\":{\"currentChannelLogin\":\"";
                string ending = "\",\"includeChanlets\":false}},";

                sb.Append(beginning);
                sb.Append(stream.Name);
                sb.Append(ending);
            }

            // to remove the unwanted comma after the last entry
            // C# 8 ranges might work here
            sb.Remove(sb.Length - 1, 1);

            sb.Append("]");

            return sb.ToString();
        }

        private void ParseJson(IEnumerable<IStream> streams, JArray results)
        {
#nullable disable
            foreach (TwitchStream stream in streams)
            {
                if (!TryGetUserDataForName(results, stream.Name, out JToken user))
                {
                    stream.Status = Status.Banned;

                    continue;
                }

                SetDisplayName(stream, user);

                bool isLive = (user["stream"].HasValues) && ((string)user["stream"]["type"] == "live");
                bool isRerun = (user["stream"].HasValues) && ((string)user["stream"]["type"] == "rerun");

                if (isLive)
                {
                    stream.ViewersCount = (int)user["stream"]["viewersCount"];

                    bool isPlayingGame = user["stream"]["game"].HasValues;

                    if (isPlayingGame)
                    {
                        int gameId = (int)user["stream"]["game"]["id"];

                        bool isUnwantedId = unwantedIds.Contains(gameId);

                        if (isUnwantedId)
                        {
                            stream.Game = string.Empty;
                            stream.Status = Status.Offline;
                        }
                        else
                        {
                            stream.Game = (string)user["stream"]["game"]["displayName"];
                            stream.Status = Status.Public;
                        }
                    }
                    else
                    {
                        stream.Status = Status.Public;
                    }
                }
                else if (isRerun)
                {
                    stream.Status = Status.Rerun;

                    LogStatic.Message($"{stream.Name} is in reruns");
                }
                else
                {
                    stream.Status = Status.Offline;
                }
            }
#nullable enable
        }

        private bool TryGetUserDataForName(JToken results, string name, out JToken? user)
        {
#nullable disable
            var tmp = results.Where(r =>
            {
                if (r.SelectToken("data.user.login", false) is JToken loginToken)
                {
                    return name == (string)loginToken;
                }
                else
                {
                    return false;
                }
            }).SingleOrDefault();

            if (tmp != null)
            {
                user = tmp["data"]["user"];

                return true;
            }
            else
            {
                user = null;

                return false;
            }
#nullable enable
        }

        private static void SetDisplayName(TwitchStream stream, JToken user)
        {
#nullable disable
            string displayName = (string)user["displayName"];

            if (!String.IsNullOrWhiteSpace(displayName)
                && stream.DisplayName != displayName)
            {
                stream.DisplayName = displayName;
            }
#nullable enable
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    download.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
