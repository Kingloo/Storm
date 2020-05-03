using System;
using System.Collections.Generic;
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
        public bool HasStreamlinkSupport { get; set; } = false;

        public MixerService(IDownload download)
        {
            this.download = download;
        }

        public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
        {
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
#nullable enable

                if ((bool)onlineToken)
                {
                    stream.Status = Status.Public;
                    stream.ViewersCount = (int)viewersToken;
                }
                else
                {
                    stream.Status = Status.Offline;
                }
                
                return Result.Success;
            }

            return Result.Failure;
        }

        public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
        {
            if (!streams.Any()) { return Result.NothingToDo; }

            List<Task<Result>> tasks = new List<Task<Result>>();

            foreach (IStream stream in streams)
            {
                Task<Result> task = Task.Run(() => UpdateAsync(stream, preserveSynchronizationContext));

                tasks.Add(task);
            }

            Result[] results = await Task.WhenAll(tasks).ConfigureAwait(preserveSynchronizationContext);

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
