using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StormLib.Common;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
    public class ChaturbateService : IService, IDisposable
    {
        private const string bannedMarker = "has been banned";

        private readonly IDownload download;

        public Type HandlesStreamType { get; } = typeof(ChaturbateStream);
        
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
                    //string result = Regex.Replace(
                    //    line,
                    //    @"\\[Uu]([0-9A-Fa-f]{4})",
                    //    m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));

                    if (line.Contains("room_status"))
                    {
                        if (line.Contains("public"))
                        {
                            stream.Status = Status.Public;
                            break;
                        }

                        if (line.Contains("offline")
                            || line.Contains("away"))
                        {
                            stream.Status = Status.Offline;
                            break;
                        }

                        if (line.Contains("private"))
                        {
                            stream.Status = Status.Private;
                            break;
                        }
                    }
                    else if (line.Contains(bannedMarker))
                    {
                        stream.Status = Status.Banned;
                        break;
                    }
                }
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
