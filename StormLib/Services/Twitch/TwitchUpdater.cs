using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;
using static StormLib.Helpers.UpdaterHelpers;

namespace StormLib.Services.Twitch
{
	public class TwitchUpdater : IUpdater<TwitchStream>
	{
		private readonly ILogger<TwitchUpdater> logger;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IOptionsMonitor<TwitchOptions> twitchOptionsMonitor;
		private readonly IOptionsMonitor<StormOptions> stormOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.Many;

		public TwitchUpdater(ILogger<TwitchUpdater> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<TwitchOptions> twitchOptionsMonitor, IOptionsMonitor<StormOptions> stormOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClientFactory);
			ArgumentNullException.ThrowIfNull(twitchOptionsMonitor);
			ArgumentNullException.ThrowIfNull(stormOptionsMonitor);

            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.twitchOptionsMonitor = twitchOptionsMonitor;
            this.stormOptionsMonitor = stormOptionsMonitor;
		}

		public Uri GetEmbeddedPlayerUriForStream(TwitchStream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);

			string format = twitchOptionsMonitor.CurrentValue.EmbeddedPlayerUriFormat;

			return new Uri(string.Format(CultureInfo.InvariantCulture, format, stream.Name), UriKind.Absolute);
		}

		public Task<Result[]> UpdateAsync(IList<TwitchStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);

		public Task<Result[]> UpdateAsync(IList<TwitchStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);

		public Task<Result[]> UpdateAsync(IList<TwitchStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);

		public async Task<Result[]> UpdateAsync(IList<TwitchStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (!streams.Any())
			{
				return Array.Empty<Result>();
			}

			IList<Result> allResults = new List<Result>();

			foreach (IList<TwitchStream> chunkOfStreams in streams.Chunk(twitchOptionsMonitor.CurrentValue.MaxStreamsPerUpdate))
			{
				Result chunkResult = await UpdateManyAsync(chunkOfStreams, preserveSynchronizationContext, cancellationToken).ConfigureAwait(preserveSynchronizationContext);

				allResults.Add(chunkResult);
			}
			
			return allResults.ToArray();
		}

		private async Task<Result> UpdateManyAsync(IList<TwitchStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			(HttpStatusCode statusCode, string text) = await RequestGraphQlDataAsync(streams, cancellationToken).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				return new Result(UpdaterType, statusCode);
			}

			if (!JsonHelpers.TryParse(text, out JsonNode? json))
			{
				return new Result(UpdaterType, statusCode)
				{
					Message = "JSON parsing failed" 
				};
			}

			foreach (JsonNode? streamData in json.AsArray())
			{
				JsonNode? userData = json?["data"]?["user"];
				string? userName = (string?)userData?["login"];

				TwitchStream? twitchStreamForThisNode = streams.SingleOrDefault(s => String.Equals(s.Name, userName, StringComparison.OrdinalIgnoreCase));

				if (twitchStreamForThisNode is null)
				{
					continue;
				}
				
				// if we find a stream to update, we remove it from the list of streams
				// then every stream left in streams is 'blanked'
				streams.Remove(twitchStreamForThisNode);

				if (TryGetDisplayName(userData, out TwitchDisplayName? displayName))
				{
					if (twitchStreamForThisNode.DisplayName != displayName.DisplayName)
					{
						twitchStreamForThisNode.DisplayName = displayName.DisplayName;
					}
				}

				if (TryGetStatus(userData, out Status status))
				{
					twitchStreamForThisNode.Status = status;
				}

				if (TryGetViewersCount(userData, out int? viewers))
				{
					twitchStreamForThisNode.ViewersCount = viewers;
				}

				if (TryGetGame(userData, out TwitchGame? game))
				{
					if (IsUnwantedGameId(game.Id))
					{
						twitchStreamForThisNode.Status = Status.Offline;
						twitchStreamForThisNode.ViewersCount = null;
						twitchStreamForThisNode.Game = null;
					}
					else
					{
						twitchStreamForThisNode.Game = game;
					}
				}
			}

			foreach (TwitchStream streamLeft in streams)
			{
				streamLeft.Status = Status.Banned;
				streamLeft.ViewersCount = null;
				streamLeft.Game = null;
			}

			return new Result(UpdaterType, statusCode);
		}

		private static bool TryGetDisplayName(JsonNode? userData, [NotNullWhen(true)] out TwitchDisplayName? displayName)
		{
			string? displayNameValue = (string?)userData?["displayName"];

			if (String.IsNullOrEmpty(displayNameValue) == false)
			{
				displayName = new TwitchDisplayName(displayNameValue);
				return true;
			}
			else
			{
				displayName = null;
				return false;
			}
		}

		private static bool TryGetStatus(JsonNode? userData, [NotNullWhen(true)] out Status status)
		{
			status = (string?)userData?["stream"]?["type"] switch
			{
				"live" => Status.Public,
				"rerun" => Status.Rerun,
				_ => Status.Problem
			};

			return true;
		}

		private static bool TryGetViewersCount(JsonNode? userData, [NotNullWhen(true)] out int? viewersCount)
		{
			viewersCount = (int?)userData?["stream"]?["viewersCount"];
			
			return viewersCount != null && viewersCount != 0;
		}

		private static bool TryGetGame(JsonNode? userData, [NotNullWhen(true)] out TwitchGame? game)
		{
			int? gameIdValue = (int?)userData?["stream"]?["game"]?["id"];
			string? gameNameValue = (string?)userData?["stream"]?["game"]?["displayName"];

			if (gameIdValue is null || gameNameValue is null)
			{
				game = null;
				return false;
			}

			game = new TwitchGame(new TwitchGameId(gameIdValue.Value), new TwitchGameName(gameNameValue));

			return true;
		}

		private bool IsUnwantedGameId(TwitchGameId gameId)
		{
			return twitchOptionsMonitor.CurrentValue.UnwantedGameIds.Contains(gameId);
		}

		private bool IsUnwantedTopicId(TwitchTopicId topicId)
		{
			return twitchOptionsMonitor.CurrentValue.UnwantedTopicIds.Contains(topicId);
		}

		private ValueTask<(HttpStatusCode, string)> RequestGraphQlDataAsync(IEnumerable<IStream> streams, CancellationToken cancellationToken)
		{
			string requestBody = BuildRequestBody(streams);

			void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				AddHeaders(twitchOptionsMonitor.CurrentValue.Headers, requestMessage);
				AddHeaders(stormOptionsMonitor.CurrentValue.CommonHeaders, requestMessage);

				requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
				requestMessage.Method = HttpMethod.Post;
				requestMessage.Version = HttpVersion.Version20;
			}

			using HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Twitch);
			
			return HttpClientHelpers.GetStringAsync(client, twitchOptionsMonitor.CurrentValue.GraphQlApiUri, ConfigureRequest, cancellationToken);
		}

		private static string BuildRequestBody(IEnumerable<IStream> streams)
		{
			StringBuilder sb = new StringBuilder();

			IList<string> queries = new List<string>();

			foreach (IStream stream in streams)
			{
				const string beginning = "{ \"query\": \"query Query($login: String) { user (login: $login) { login displayName description primaryColorHex roles { isAffiliate isPartner } profileImageURL(width: 70) offlineImageURL freeformTags { id name } stream { createdAt viewersCount isEncrypted previewImageURL(width: 1280, height: 720) type isMature language game { id name displayName } } } }\", \"variables\":{\"login\":\"";
				const string ending = "\"} }";

				sb.Append(beginning);
				sb.Append(stream.Name);
				sb.Append(ending);

				queries.Add(sb.ToString());

				sb.Clear();
			}

			return $"[{String.Join(", ", queries)}]";
		}
	}
}
