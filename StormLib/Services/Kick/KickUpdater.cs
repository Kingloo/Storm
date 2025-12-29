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
using static StormLib.Helpers.UpdaterHelpers;

namespace StormLib.Services.Kick
{
	public class KickUpdater : IUpdater<KickStream>, IDisposable
	{
		private readonly ILogger<KickUpdater> logger;
		private readonly HttpClient httpClient;
		private readonly IOptionsMonitor<KickOptions> kickOptionsMonitor;
		private readonly IOptionsMonitor<StormOptions> stormOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public KickUpdater(
			ILogger<KickUpdater> logger,
			HttpClient httpClient,
			IOptionsMonitor<KickOptions> kickOptionsMonitor,
			IOptionsMonitor<StormOptions> stormOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClient);
			ArgumentNullException.ThrowIfNull(kickOptionsMonitor);
			ArgumentNullException.ThrowIfNull(stormOptionsMonitor);

			this.logger = logger;
			this.httpClient = httpClient;
			this.kickOptionsMonitor = kickOptionsMonitor;
			this.stormOptionsMonitor = stormOptionsMonitor;
		}

		public Task<IReadOnlyList<Result<KickStream>>> UpdateAsync(IReadOnlyList<KickStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task<IReadOnlyList<Result<KickStream>>> UpdateAsync(IReadOnlyList<KickStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (streams.Count == 0)
			{
				return Array.Empty<Result<KickStream>>();
			}

			if (streams.Count == 1)
			{
				Result<KickStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);

				return new[] { singleResult };
			}

			return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
		}

		private async Task<Result<KickStream>> UpdateOneAsync(KickStream stream, CancellationToken cancellationToken)
		{
			logger.LogDebug("update '{}'", stream.DisplayName);

			Uri apiEndpointForStream = new Uri($"{kickOptionsMonitor.CurrentValue.ApiUri}/channels/{stream.Name}", UriKind.Absolute);

			void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				requestMessage.Headers.Host = "kick.com";
				requestMessage.Method = HttpMethod.Get;
				requestMessage.Version = HttpVersion.Version20;

				AddHeaders(kickOptionsMonitor.CurrentValue.Headers, requestMessage);
				AddHeaders(stormOptionsMonitor.CurrentValue.CommonHeaders, requestMessage);
			}

			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			(statusCode, text) = await HttpClientHelpers.GetStringAsync(httpClient, apiEndpointForStream, ConfigureRequest, cancellationToken).ConfigureAwait(false);

			if (statusCode != HttpStatusCode.OK)
			{
				return new Result<KickStream>(stream)
				{
					Action = static (KickStream k) =>
					{
						k.Status = Status.Problem;
						k.ViewersCount = null;
						k.SessionTitle = null;
					},
					StatusCode = statusCode
				};
			}

			if (!JsonHelpers.TryParse(text, out JsonNode? json))
			{
				return new Result<KickStream>(stream)
				{
					Action = static (KickStream k) =>
					{
						k.Status = Status.Problem;
						k.ViewersCount = null;
						k.SessionTitle = null;
					},
					StatusCode = statusCode
				};
			}

			string? newDisplayName = json?["user"]?["username"] is JsonNode displayNameToken ? (string?)displayNameToken : null;

			bool? isPublic = (bool?)json?["livestream"]?["is_live"];

			int? newViewersCount = isPublic.HasValue switch
			{
				true => json?["livestream"]?["viewer_count"] is JsonNode viewerCountToken ? (int?)viewerCountToken : null,
				false => null
			};

			string? newSessionTitle = isPublic.HasValue switch
			{
				true => json?["livestream"]?["session_title"] is JsonNode sessionTitleToken ? (string?)sessionTitleToken : null,
				false => null
			};

			Status newStatus = isPublic.HasValue switch
			{
				true => isPublic.Value ? Status.Public : Status.Offline,
				false => Status.Offline
			};

			return new Result<KickStream>(stream)
			{
				Action = (KickStream k) =>
				{
					if (String.IsNullOrEmpty(newDisplayName) == false
						&& String.Equals(newDisplayName, stream.DisplayName, StringComparison.Ordinal) == false)
					{
						k.DisplayName = newDisplayName;
					}

					k.Status = newStatus;
					k.ViewersCount = newViewersCount;
					k.SessionTitle = newSessionTitle;
				},
				StatusCode = statusCode
			};
		}

		private Task<Result<KickStream>[]> UpdateManyAsync(IReadOnlyList<KickStream> streams, CancellationToken cancellationToken)
		{
			List<Task<Result<KickStream>>> updateTasks = new List<Task<Result<KickStream>>>();

			foreach (KickStream each in streams)
			{
				Task<Result<KickStream>> updateTask = Task.Run(() => UpdateOneAsync(each, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					httpClient.Dispose();
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
