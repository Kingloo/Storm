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
    public class MixlrService : IService
    {
        private readonly IDownload download;

        public Type HandlesStreamType { get; } = typeof(MixlrStream);
        public bool HasStreamlinkSupport { get; set; } = false;

        public MixlrService(IDownload download)
        {
            this.download = download;
        }

        public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
        {
            UriBuilder apiCall = new UriBuilder
            {
                Host = "api.mixlr.com",
                Path = $"/users/{stream.Name}",
                Port = 443,
                Scheme = "https://"
            };

            (HttpStatusCode status, string text) = await download.StringAsync(apiCall.Uri).ConfigureAwait(preserveSynchronizationContext);

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

            if (json.TryGetValue("username", out JToken usernameToken)
                && json.TryGetValue("is_live", out JToken isLiveToken))
            {
                string username = (string)usernameToken;

                if (stream.DisplayName != username)
                {
                    stream.DisplayName = username;
                }
#nullable enable

                stream.Status = (bool)isLiveToken ? Status.Public : Status.Offline;

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
                Task<Result> task = Task<Result>.Run(() => UpdateAsync(stream, preserveSynchronizationContext));

                tasks.Add(task);
            }

            Result[] results = await Task.WhenAll(tasks).ConfigureAwait(preserveSynchronizationContext);

            return results.OrderByDescending(r => r).First();
        }
    }
}
