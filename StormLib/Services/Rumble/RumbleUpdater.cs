using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services.Rumble
{
	public class RumbleUpdater : IUpdater<RumbleStream>
	{
		private const string liveMarker = "data-value=\"LIVE\"";

		private readonly ILogger<RumbleUpdater> logger;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IOptionsMonitor<RumbleOptions> rumbleOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public RumbleUpdater(ILogger<RumbleUpdater> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<RumbleOptions> rumbleOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClientFactory);
			ArgumentNullException.ThrowIfNull(rumbleOptionsMonitor);

			this.logger = logger;
			this.httpClientFactory = httpClientFactory;
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
			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;
			
			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Rumble))
			{
				(statusCode, text) = await Helpers.HttpClientHelpers.GetStringAsync(client, stream.Link, cancellationToken).ConfigureAwait(preserveSynchronizationContext);
			}

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