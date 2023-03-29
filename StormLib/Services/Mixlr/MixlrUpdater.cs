using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services.Mixlr
{
	public class MixlrUpdater : IUpdater<MixlrStream>
	{
		private readonly ILogger<MixlrUpdater> logger;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IOptionsMonitor<MixlrOptions> mixlrOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public MixlrUpdater(ILogger<MixlrUpdater> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<MixlrOptions> mixlrOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClientFactory);
			ArgumentNullException.ThrowIfNull(mixlrOptionsMonitor);

			this.logger = logger;
			this.httpClientFactory = httpClientFactory;
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
			Uri uri = new Uri($"{mixlrOptionsMonitor.CurrentValue.ApiUri}/users/{stream.Name}", UriKind.Absolute);

			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Mixlr))
			{
				(statusCode, text) = await HttpClientHelpers.GetStringAsync(client, uri, cancellationToken).ConfigureAwait(preserveSynchronizationContext);
			}

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
					Message = "JSON parsing failed"
				};
			}

			JsonNode? userNameToken = json?["username"];
			JsonNode? isLiveToken = json?["is_live"];

			if (userNameToken is null)
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode)
				{
					Message = "token did not exist: 'username'"
				};
			}

			if (isLiveToken is null)
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode)
				{
					Message = "token did not exist: 'is_live'"
				};
			}

			string? userName = (string?)userNameToken;

			if (String.IsNullOrWhiteSpace(userName) == false && stream.DisplayName != userName)
			{
				stream.DisplayName = userName;
			}

			stream.Status = (bool)isLiveToken ? Status.Public : Status.Offline;

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
