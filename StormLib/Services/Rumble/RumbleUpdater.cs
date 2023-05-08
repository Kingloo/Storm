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
using static StormLib.Helpers.HttpStatusCodeHelpers;

namespace StormLib.Services.Rumble
{
	public class RumbleUpdater : IUpdater<RumbleStream>
	{
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

		public Task<IList<Result<RumbleStream>>> UpdateAsync(IReadOnlyList<RumbleStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task<IList<Result<RumbleStream>>> UpdateAsync(IReadOnlyList<RumbleStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (!streams.Any())
			{
				return Array.Empty<Result<RumbleStream>>();
			}

			if (streams.Count == 1)
			{
				Result<RumbleStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);

				return new[] { singleResult };
			}
			else
			{
				return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
			}
		}

		private async Task<Result<RumbleStream>> UpdateOneAsync(RumbleStream stream, CancellationToken cancellationToken)
		{
			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			Status newStatus = Status.Unknown;
			int? newViewersCount = null;

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Rumble))
			{
				(statusCode, text) = await Helpers.HttpClientHelpers.GetStringAsync(client, stream.Link, cancellationToken).ConfigureAwait(false);
			}

			if (statusCode == HttpStatusCode.OK) // change to allow for 307 TempRedir
			{
				Range liveMarkerSearchRange = new Range(10_000, text.Length - 10_000); // not within the first X characters nor the last Y characters

				bool containsLiveMarker = text[liveMarkerSearchRange].Contains(rumbleOptionsMonitor.CurrentValue.LiveMarker, StringComparison.OrdinalIgnoreCase);

				newStatus = containsLiveMarker ? Status.Public : Status.Offline;
			}
			else
			{
				logger.LogWarning("getting account page for {AccountName} on Rumble returned {StatusCode}, setting status to Offline", stream.DisplayName, FormatStatusCode(statusCode));

				newStatus = Status.Offline;
				newViewersCount = null;
			}

			return new Result<RumbleStream>(stream, statusCode)
			{
				Action = (RumbleStream r) =>
				{
					r.Status = newStatus;
					r.ViewersCount = newViewersCount;
				}
			};
		}

		private Task<Result<RumbleStream>[]> UpdateManyAsync(IReadOnlyList<RumbleStream> streams, CancellationToken cancellationToken)
		{
			var updateTasks = new List<Task<Result<RumbleStream>>>();

			foreach (RumbleStream each in streams)
			{
				Task<Result<RumbleStream>> updateTask = Task.Run(() => UpdateOneAsync(each, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}
