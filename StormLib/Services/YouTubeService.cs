using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StormLib.Extensions;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
    public class YouTubeService : IService, IDisposable
    {
        private readonly IDownload download;

        public Type HandlesStreamType => typeof(YouTubeStream);

        public YouTubeService(IDownload download)
        {
            this.download = download;
        }

        public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
        {
            (HttpStatusCode status, string text) = await download.StringAsync(new Uri($"{stream.Link.AbsoluteUri}?ucbcb=1")).ConfigureAwait(preserveSynchronizationContext);

            if (status != HttpStatusCode.OK)
            {
                return Result.WebFailure;
            }

            stream.DisplayName = SetDisplayName(text, stream.Name);

            if (text.Contains("\"style\":\"LIVE\",\"icon\":{\"iconType\":\"LIVE\"}"))
            {
                stream.Status = Status.Public;
            }
            else
            {
                stream.Status = Status.Offline;
            }

            return Result.Success;
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

        private static string SetDisplayName(string text, string fallback)
        {
            // "twitter:title" content="Eris In Progress">
            // "twitter:title" content="
            // ">
            // Eris In Progress

            string beginning = "\"twitter:title\" content=\"";
            string ending = "\">";

            return text.FindBetween(beginning, ending).FirstOrDefault() ?? fallback;
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
