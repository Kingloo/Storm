using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

			(HttpStatusCode status, string text) = await download.StringAsync(new Uri($"{stream.Link.AbsoluteUri}?ucbcb=1")).ConfigureAwait(preserveSynchronizationContext);

			if (status != HttpStatusCode.OK)
			{
				return Result.WebFailure;
			}

			stream.DisplayName = SetDisplayName(text, stream.Name);
            stream.Status = SetStatus(text);
            stream.ViewersCount = SetViewers(text);

			return Result.Success;
		}

		public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
		{
			if (streams is null)
			{
				throw new ArgumentNullException(nameof(streams));
			}

			IList<IStream> enumeratedStreams = streams.ToList<IStream>();

			if (!enumeratedStreams.Any())
			{
				return Result.NothingToDo;
			}

			List<Task<Result>> tasks = new List<Task<Result>>();

			foreach (IStream stream in enumeratedStreams)
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

			const string beginning = "\"twitter:title\" content=\"";
			const string ending = "\">";

			return text.FindBetween(beginning, ending).FirstOrDefault() ?? fallback;
		}

        private static Status SetStatus(string text)
        {
			const string liveMarker = "\"style\":\"LIVE\",\"icon\":{\"iconType\":\"LIVE\"}";

            if (text.Contains(liveMarker, StringComparison.OrdinalIgnoreCase))
			{
				return Status.Public;
			}
			else
			{
				return Status.Offline;
			}
        }

        private static Nullable<int> SetViewers(string text)
        {
            const string beginning = "viewCountText\":{\"runs\":[{\"text\":\"";
            const string ending = "\"}";

            string? viewersText = text.FindBetween(beginning, ending).FirstOrDefault();

			if (viewersText is null)
			{
				return null;
			}

			string viewersTextDigitsOnly = GetOnlyDigits(viewersText);

            if (Int32.TryParse(viewersTextDigitsOnly, out int result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

		private static string GetOnlyDigits(string text)
		{
			return new StringBuilder()
				.Append(text.Trim().Where(static c => Char.IsDigit(c)).ToArray())
				.ToString();
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
