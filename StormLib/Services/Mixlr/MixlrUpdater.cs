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

		public Task<IReadOnlyList<Result<MixlrStream>>> UpdateAsync(IReadOnlyList<MixlrStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task<IReadOnlyList<Result<MixlrStream>>> UpdateAsync(IReadOnlyList<MixlrStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (!streams.Any())
			{
				return Array.Empty<Result<MixlrStream>>();
			}

			if (streams.Count == 1)
			{
				Result<MixlrStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);

				return new[] { singleResult };
			}
			else
			{
				return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
			}
		}

		private async Task<Result<MixlrStream>> UpdateOneAsync(MixlrStream stream, CancellationToken cancellationToken)
		{
			logger.LogDebug("update '{}'", stream.DisplayName);

			Uri uri = new Uri($"{mixlrOptionsMonitor.CurrentValue.ApiUri}/users/{stream.Name}", UriKind.Absolute);

			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Mixlr))
			{
				(statusCode, text) = await HttpClientHelpers.GetStringAsync(client, uri, cancellationToken).ConfigureAwait(false);
			}

			if (statusCode != HttpStatusCode.OK)
			{
				return new Result<MixlrStream>(stream)
				{
					Action = static (MixlrStream m) =>
					{
						m.Status = Status.Problem;
						m.ViewersCount = null;
					},
					StatusCode = statusCode
				};
			}

			if (!JsonHelpers.TryParse(text, out JsonNode? json))
			{
				return new Result<MixlrStream>(stream)
				{
					Action = static (MixlrStream m) =>
					{
						m.Status = Status.Problem;
						m.ViewersCount = null;
					},
					Message = "JSON parsing failed",
					StatusCode = statusCode
				};
			}

			JsonNode? userNameToken = json?["username"];
			JsonNode? isLiveToken = json?["is_live"];

			if (userNameToken is null)
			{
				return new Result<MixlrStream>(stream)
				{
					Action = static (MixlrStream m) =>
					{
						m.Status = Status.Problem;
						m.ViewersCount = null;
					},
					Message = "token did not exist: 'username'",
					StatusCode = statusCode
				};
			}

			if (isLiveToken is null)
			{
				return new Result<MixlrStream>(stream)
				{
					Action = static (MixlrStream m) =>
					{
						m.Status = Status.Problem;
						m.ViewersCount = null;
					},
					Message = "token did not exist: 'is_live'",
					StatusCode = statusCode
				};
			}

			string? userName = (string?)userNameToken;

			return new Result<MixlrStream>(stream)
			{
				Action = (MixlrStream m) =>
				{
					if (String.IsNullOrWhiteSpace(userName) == false
						&& String.Equals(userName, stream.DisplayName, StringComparison.Ordinal) == false)
					{
						m.DisplayName = userName;
					}

					m.Status = (bool)isLiveToken ? Status.Public : Status.Offline;
				},
				StatusCode = statusCode
			};
		}

		private Task<Result<MixlrStream>[]> UpdateManyAsync(IReadOnlyList<MixlrStream> streams, CancellationToken cancellationToken)
		{
			List<Task<Result<MixlrStream>>> updateTasks = new List<Task<Result<MixlrStream>>>();

			foreach (MixlrStream each in streams)
			{
				Task<Result<MixlrStream>> updateTask = Task.Run(() => UpdateOneAsync(each, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}
