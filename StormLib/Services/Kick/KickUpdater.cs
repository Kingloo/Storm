using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;
using static StormLib.Helpers.UpdaterHelpers;

namespace StormLib.Services.Kick
{
	public class KickUpdater : IUpdater<KickStream>
	{
		private readonly ILogger<KickUpdater> logger;
		private readonly IOptionsMonitor<KickOptions> kickOptionsMonitor;
		private readonly IOptionsMonitor<StormOptions> stormOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public KickUpdater(ILogger<KickUpdater> logger, IOptionsMonitor<KickOptions> kickOptionsMonitor, IOptionsMonitor<StormOptions> stormOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(kickOptionsMonitor);
			ArgumentNullException.ThrowIfNull(stormOptionsMonitor);

			this.logger = logger;
			this.kickOptionsMonitor = kickOptionsMonitor;
			this.stormOptionsMonitor = stormOptionsMonitor;
		}

		public Task<Result[]> UpdateAsync(IList<KickStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);

		public Task<Result[]> UpdateAsync(IList<KickStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);

		public Task<Result[]> UpdateAsync(IList<KickStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);
		
		public async Task<Result[]> UpdateAsync(IList<KickStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
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

		private async Task<Result> UpdateOneAsync(KickStream stream, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			Uri apiEndpointForStream = new Uri($"{kickOptionsMonitor.CurrentValue.ApiUri}/channels/{stream.Name}", UriKind.Absolute);

			void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				AddHeaders(kickOptionsMonitor.CurrentValue.headers, requestMessage);
				AddHeaders(stormOptionsMonitor.CurrentValue.headers, requestMessage);

				requestMessage.Headers.Host = "kick.com";
				requestMessage.Method = HttpMethod.Get;
				requestMessage.Version = HttpVersion.Version20;
			};

			(HttpStatusCode statusCode, string text) = await download.StringAsync(apiEndpointForStream, ConfigureRequest).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode);
			}

			if (!JsonHelpers.TryParse(text, out JsonNode? json))
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;
				
				return new Result(UpdaterType, statusCode);
			}

			if (json?["user"]?["username"] is JsonNode displayNameToken)
			{
				if ((string?)displayNameToken is string displayNameValue)
				{
					stream.DisplayName = displayNameValue;
				}
			}

			if (json?["livestream"]?.Options.HasValue ?? false)
			{
				stream.Status = Status.Public;

				if (json?["livestream"]?["viewer_count"] is JsonNode viewerCountToken)
				{
					stream.ViewersCount = (int)viewerCountToken;
				}
			}
			else
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;
			}

			return new Result(UpdaterType, statusCode);
		}

		private Task<Result[]> UpdateManyAsync(IList<KickStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (KickStream each in streams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateOneAsync(each, preserveSynchronizationContext, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}