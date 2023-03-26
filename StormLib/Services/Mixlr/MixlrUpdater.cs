using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services.Mixlr
{
	public class MixlrUpdater : IUpdater<MixlrStream>
	{
		private readonly ILogger<MixlrUpdater> logger;
		private readonly IOptionsMonitor<MixlrOptions> mixlrOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public MixlrUpdater(ILogger<MixlrUpdater> logger, IOptionsMonitor<MixlrOptions> mixlrOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(mixlrOptionsMonitor);

			this.logger = logger;
			this.mixlrOptionsMonitor = mixlrOptionsMonitor;
		}

		public Task<Result[]> UpdateAsync(IList<MixlrStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);
		
		public Task<Result[]> UpdateAsync(IList<MixlrStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);
		
		public Task<Result[]> UpdateAsync(IList<MixlrStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);

		public async Task<Result[]> UpdateAsync(IList<MixlrStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
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

		private async Task<Result> UpdateOneAsync(MixlrStream stream, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			Uri api = new Uri($"{mixlrOptionsMonitor.CurrentValue.ApiUri}/users/{stream.Name}", UriKind.Absolute);

			(HttpStatusCode statusCode, string text) = await download.StringAsync(api).ConfigureAwait(preserveSynchronizationContext);

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

				return new Result(UpdaterType, statusCode)
				{
					Message = "json parsing failed"
				};
			}

			if (json?["username"] is JsonNode usernameToken)
			{
				string? username = (string?)usernameToken;

				if (String.IsNullOrWhiteSpace(username) == false && stream.DisplayName != username)
				{
					stream.DisplayName = username;
				}
			}

			if (json?["is_live"] is JsonNode isLiveToken)
			{
				stream.Status = (bool)isLiveToken ? Status.Public : Status.Offline;
			}

			return new Result(UpdaterType, statusCode);
		}

		private Task<Result[]> UpdateManyAsync(IList<MixlrStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (MixlrStream each in streams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateOneAsync(each, preserveSynchronizationContext, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}
