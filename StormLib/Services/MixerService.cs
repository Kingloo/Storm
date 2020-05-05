using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StormLib.Common;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
    public class MixerService : IService, IDisposable
    {
        private readonly IDownload download;

        public Type HandlesStreamType { get; } = typeof(MixerStream);

        public MixerService(IDownload download)
        {
            this.download = download;
        }

        public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
        {
            LogStatic.Message($"top: {stream.Name}");

            UriBuilder apiUri = new UriBuilder
            {
                Host = "mixer.com",
                Path = $"/api/v1/channels/{stream.Name}",
                Port = 443,
                Scheme = "https://"
            };

            (HttpStatusCode status, string text) = await download.StringAsync(apiUri.Uri).ConfigureAwait(preserveSynchronizationContext);

            if (status != HttpStatusCode.OK)
            {
                return Result.WebFailure;
            }

            if (!Json.TryParse(text, out JObject? json))
            {
                return Result.ParsingJsonFailed;
            }

#nullable disable
            if (!json.HasValues)
            {
                return Result.Failure;
            }

            LogStatic.Message(text);

            if (json.TryGetValue("token", out JToken userNameToken)
                && json.TryGetValue("online", out JToken onlineToken)
                && json.TryGetValue("viewersCurrent", out JToken viewersToken))
            {
                string displayName = (string)userNameToken;

                if (!String.IsNullOrWhiteSpace(displayName)
                    && stream.DisplayName != displayName)
                {
                    stream.DisplayName = displayName;
                }

                if ((bool)onlineToken)
                {
                    stream.ViewersCount = (int)viewersToken;
                    stream.Status = Status.Public;

                    if (json.SelectToken("type.name", false) is JToken gameToken)
                    {
                        (stream as MixerStream).Game = (string)gameToken;
                    }
                }
                else
                {
                    stream.ViewersCount = -1;
                    stream.Status = Status.Offline;
                    (stream as MixerStream).Game = string.Empty;
                }
#nullable enable

                return Result.Success;
            }

            return Result.Failure;
        }

        public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
        {
            /*
             * Doing these simultaneously in the way the others do didn't work.
             * Don't know why!
             * One would start, but then the other would finish.
             * I observed both finishing properly only once.
             * So we do them sequentially.
             */

            if (!streams.Any()) { return Result.NothingToDo; }

            Collection<Result> results = new Collection<Result>();

            foreach (IStream each in streams)
            {
                Result result = await UpdateAsync(each, preserveSynchronizationContext).ConfigureAwait(preserveSynchronizationContext);

                results.Add(result);
            }

            return results.OrderByDescending(r => r).First();
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
