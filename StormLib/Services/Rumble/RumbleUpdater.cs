using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services.Rumble
{
	public class RumbleUpdater : IUpdater<RumbleStream>
	{
		private const string liveMarker = "data-value=\"LIVE\"";

		private readonly ILogger<RumbleUpdater> logger;
		private readonly IOptionsMonitor<RumbleOptions> rumbleOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public RumbleUpdater(ILogger<RumbleUpdater> logger, IOptionsMonitor<RumbleOptions> rumbleOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(rumbleOptionsMonitor);

			this.logger = logger;
			this.rumbleOptionsMonitor = rumbleOptionsMonitor;
		}

		public Task<Result[]> UpdateAsync(IList<RumbleStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);
		
		public Task<Result[]> UpdateAsync(IList<RumbleStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);
		
		public Task<Result[]> UpdateAsync(IList<RumbleStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);

		public async Task<Result[]> UpdateAsync(IList<RumbleStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (!streams.Any())
			{
				return Array.Empty<Result>();
			}

			if (streams.Count == 1)
			{
				Result singleResult = await UpdateOneAsync(streams[0], preserveSynchronizationContext, cancellationToken).ConfigureAwait(preserveSynchronizationContext);
				
				return new [] { singleResult };
			}
			else
			{
				return await UpdateManyAsync(streams, preserveSynchronizationContext, cancellationToken).ConfigureAwait(preserveSynchronizationContext);
			}
		}

		private async Task<Result> UpdateOneAsync(RumbleStream stream, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			(HttpStatusCode statusCode, string text) = await download.StringAsync(stream.Link).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode);
			}

			Range liveMarkerSearchRange = new Range(10_000, text.Length - 10_000); // not within the first X characters nor the last Y characters

			bool containsLiveMarker = text[liveMarkerSearchRange].Contains(liveMarker, StringComparison.OrdinalIgnoreCase);

			stream.Status = containsLiveMarker ? Status.Public : Status.Offline;

			return new Result(UpdaterType, statusCode);
		}

		private Task<Result[]> UpdateManyAsync(IList<RumbleStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (RumbleStream each in streams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateOneAsync(each, preserveSynchronizationContext, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}