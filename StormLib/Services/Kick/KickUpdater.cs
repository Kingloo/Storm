using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib.Common;
using StormLib.Helpers;
using StormLib.Interfaces;
using static StormLib.Helpers.UpdaterHelpers;

namespace StormLib.Services.Kick
{
	public class KickUpdater : IUpdater<KickStream>
	{
		private readonly ILogger<KickUpdater> logger;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IOptionsMonitor<KickOptions> kickOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public KickUpdater(
			ILogger<KickUpdater> logger,
			IHttpClientFactory httpClientFactory,
			IOptionsMonitor<KickOptions> kickOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClientFactory);
			ArgumentNullException.ThrowIfNull(kickOptionsMonitor);

			this.logger = logger;
			this.httpClientFactory = httpClientFactory;
			this.kickOptionsMonitor = kickOptionsMonitor;
		}

		public Task<IList<Result<KickStream>>> UpdateAsync(IReadOnlyList<KickStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task<IList<Result<KickStream>>> UpdateAsync(IReadOnlyList<KickStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (!streams.Any())
			{
				return Array.Empty<Result<KickStream>>();
			}

			if (streams.Count == 1)
			{
				Result<KickStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);
				
				return new [] { singleResult };
			}
			else
			{
				return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
			}
		}

		private async Task<Result<KickStream>> UpdateOneAsync(KickStream stream, CancellationToken cancellationToken)
		{
			Uri apiEndpointForStream = new Uri($"{kickOptionsMonitor.CurrentValue.ApiUri}/channels/{stream.Name}", UriKind.Absolute);

			void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				requestMessage.Headers.Host = "kick.com";
				requestMessage.Method = HttpMethod.Get;
				requestMessage.Version = HttpVersion.Version20;

				AddHeaders(kickOptionsMonitor.CurrentValue.Headers, requestMessage);
			};

			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Kick))
			{
				(statusCode, text) = await HttpClientHelpers.GetStringAsync(client, apiEndpointForStream, ConfigureRequest, cancellationToken).ConfigureAwait(false);
			}

			if (statusCode != HttpStatusCode.OK)
			{
				return new Result<KickStream>(stream, statusCode)
				{
					Action = (KickStream k) =>
					{
						k.Status = Status.Problem;
						k.ViewersCount = null;
					}
				};
			}

			if (!JsonHelpers.TryParse(text, out JsonNode? json))
			{
				return new Result<KickStream>(stream, statusCode)
				{
					Action = (KickStream k) =>
					{
						k.Status = Status.Problem;
						k.ViewersCount = null;
					}
				};
			}

			string? newDisplayName = json?["user"]?["username"] is JsonNode displayNameToken
				? (string?)displayNameToken
				: null;			
			
			bool isPublic = json?["livestream"]?.Options.HasValue ?? false;
			
			int? newViewersCount = isPublic switch
			{
				true => json?["livestream"]?["viewer_count"] is JsonNode viewerCountToken ? (int?)viewerCountToken : null,
				false => null
			};

			Status newStatus = isPublic
				? Status.Public
				: Status.Offline;

			return new Result<KickStream>(stream, statusCode)
			{
				Action = (KickStream k) =>
				{
					if (newDisplayName != null)
					{
						k.DisplayName = newDisplayName;
					}
					
					k.Status = Status.Public;
					
					k.ViewersCount = newViewersCount;
				}
			};
		}

		private Task<Result<KickStream>[]> UpdateManyAsync(IReadOnlyList<KickStream> streams, CancellationToken cancellationToken)
		{
			IList<Task<Result<KickStream>>> updateTasks = new List<Task<Result<KickStream>>>();

			foreach (KickStream each in streams)
			{
				Task<Result<KickStream>> updateTask = Task.Run(() => UpdateOneAsync(each, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}