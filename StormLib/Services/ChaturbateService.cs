using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
    public class ChaturbateService : IService, IDisposable
    {
        private const string offlineMarker = "Room is currently offline";
        private const string loginMarker = "meta name=\"keywords\" content=\"Login, Chaturbate login\"";
        private const string bannedMarker = "has been banned";

        private readonly IDownload download;

        public Type HandlesStreamType { get; } = typeof(ChaturbateStream);
        public bool HasStreamlinkSupport { get; set; } = false;
        
        public ChaturbateService(IDownload download)
        {
            this.download = download;
        }

        public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
        {
            (HttpStatusCode status, string text) = await download.StringAsync(stream.Link).ConfigureAwait(preserveSynchronizationContext);

            if (status != HttpStatusCode.OK)
            {
                return Result.WebFailure;
            }

            using (StringReader sr = new StringReader(text))
            {
                string line = string.Empty;

                while ((line = await sr.ReadLineAsync().ConfigureAwait(preserveSynchronizationContext)) != null)
                {
                    if (line.Contains(offlineMarker))
                    {
                        stream.Status = Status.Offline;
                        return Result.Success;
                    }

                    if (line.Contains(loginMarker))
                    {
                        stream.Status = Status.Private;
                        return Result.Success;
                    }

                    if (line.Contains(bannedMarker))
                    {
                        stream.Status = Status.Banned;
                        return Result.Success;
                    }
                }
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
