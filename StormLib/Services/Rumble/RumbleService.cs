using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
	public class RumbleService : IService, IDisposable
	{
		private const string liveMarker = "data-value=\"LIVE\"";

		public Type HandlesStreamType { get => typeof(RumbleStream); }

		private readonly IDownload download;

		public RumbleService(IDownload download)
		{
			ArgumentNullException.ThrowIfNull(download);

			this.download = download;
		}

		public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
		{
			ArgumentNullException.ThrowIfNull(stream);

			(HttpStatusCode status, string text) = await download.StringAsync(stream.Link).ConfigureAwait(preserveSynchronizationContext);

			if (status != HttpStatusCode.OK)
			{
				return Result.WebFailure;
			}

			bool containsLiveMarker = text.Contains(liveMarker, StringComparison.OrdinalIgnoreCase);

			stream.Status = containsLiveMarker switch
			{
				true => Status.Public,
				false => Status.Offline
			};

			return Result.Success;
		}

		public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
		{
			ArgumentNullException.ThrowIfNull(streams);

			IList<IStream> enumeratedStreams = streams.ToList();

			if (!enumeratedStreams.Any())
			{
				return Result.NothingToDo;
			}

			List<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (IStream each in enumeratedStreams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateAsync(each, preserveSynchronizationContext));

				updateTasks.Add(updateTask);
			}

			Result[] results = await Task.WhenAll(updateTasks).ConfigureAwait(preserveSynchronizationContext);

			return results.OrderByDescending(static r => r).First();
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
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}