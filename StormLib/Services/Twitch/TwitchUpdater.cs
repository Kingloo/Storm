using System;
using System.Collections.Generic;
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
using StormLib.Exceptions;
using StormLib.Helpers;
using StormLib.Interfaces;
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

		public TwitchUpdater(
			ILogger<TwitchUpdater> logger,
			IHttpClientFactory httpClientFactory,
			IOptionsMonitor<TwitchOptions> twitchOptionsMonitor,
			IOptionsMonitor<StormOptions> stormOptionsMonitor)
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

		public Task<IList<Result<TwitchStream>>> UpdateAsync(IReadOnlyList<TwitchStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public Task<IList<Result<TwitchStream>>> UpdateAsync(IReadOnlyList<TwitchStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			logger.LogDebug("updating {Count} {StreamPluralized}", streams.Count, streams.Count == 1 ? "stream" : "streams");

			return UpdateManyAsync(streams, cancellationToken);
		}

		private async Task<IList<Result<TwitchStream>>> UpdateManyAsync(IReadOnlyList<TwitchStream> streams, CancellationToken cancellationToken)
		{
			(HttpStatusCode statusCode, string text) = await RequestGraphQlDataAsync(streams, cancellationToken).ConfigureAwait(false);

			if (statusCode != HttpStatusCode.OK)
			{
				return streams
					.Select(s => new Result<TwitchStream>(s, statusCode)
					{
						Action = BlankTwitchStream,
						Message = "status code was not OK"
					})
					.ToList();
			}

			if (!JsonHelpers.TryParse(text, out JsonNode? json))
			{
				return streams
					.Select(s => new Result<TwitchStream>(s, statusCode)
					{
						Action = BlankTwitchStream,
						Message = "JSON parsing failed"
					})
					.ToList();
			}

			if (json is not JsonArray jsonArray)
			{
				throw new System.Text.Json.JsonException("Twitch data format has changed: JsonNode is not an array");
			}

			IList<Result<TwitchStream>> results = new List<Result<TwitchStream>>();

			foreach (TwitchStream each in streams)
			{
				JsonNode? data = jsonArray
					.SingleOrDefault((JsonNode? element) =>
					{
						bool elementHasUserData = false;
						bool doesUserDataLoginMatchStreamName = false;

						if (element?["data"]?["user"] is JsonNode node)
						{
							elementHasUserData = true;

							doesUserDataLoginMatchStreamName = String.Equals((string?)node["login"], each.Name, StringComparison.OrdinalIgnoreCase);
						}

						return elementHasUserData && doesUserDataLoginMatchStreamName;
					});

				JsonNode? userData = data?["data"]?["user"];

				Action<TwitchStream> action;

				if (userData is null)
				{
					action = (TwitchStream t) =>
					{
						// Twitch's GraphQl API does not distinguish between does-not-exist, banned or closed so we default to Banned for all possibilities
						// don't blank or reset DisplayName: this allows it to persist while the programme is open

						t.Status = Status.Banned;
						t.ViewersCount = null;
						t.Game = null;
					};
				}
				else
				{
					TwitchGame? newGame = GetGame(userData);

					action = IsUnwantedGameId(newGame?.Id) switch
					{
						true => (TwitchStream t) => // don't use BlankTwitchStream because we still want to update DisplayName
						{
							t.DisplayName = GetDisplayName(userData) ?? each.Name;
							t.Status = Status.Offline;
							t.ViewersCount = null;
							t.Game = null;
						},
						false => (TwitchStream t) =>
						{
							t.DisplayName = GetDisplayName(userData) ?? each.Name;
							t.Status = GetStatus(userData);
							t.ViewersCount = GetViewersCount(userData);
							t.Game = newGame;
						}
					};
				}

				Result<TwitchStream> result = new Result<TwitchStream>(each, statusCode, action);

				results.Add(result);
			}

			return results;
		}

		private static void BlankTwitchStream(TwitchStream twitchStream)
		{
			twitchStream.Status = Status.Offline;
			twitchStream.ViewersCount = null;
			twitchStream.Game = null;
		}

		private static string? GetDisplayName(JsonNode? userData)
		{
			return (string?)userData?["displayName"];
		}

		private static Status GetStatus(JsonNode? userData)
		{
			return (string?)userData?["stream"]?["type"] switch
			{
				"live" => Status.Public,
				"rerun" => Status.Rerun,
				_ => Status.Offline
			};
		}

		private static int? GetViewersCount(JsonNode? userData)
		{
			return (int?)userData?["stream"]?["viewersCount"];
		}

		private static TwitchGame? GetGame(JsonNode? userData)
		{
			Int64? gameId = Int64.TryParse((string?)userData?["stream"]?["game"]?["id"], out Int64 id) ? id : null;
			string? gameName = (string?)userData?["stream"]?["game"]?["displayName"];

			if (gameId is null || gameName is null)
			{
				return null;
			}

			return new TwitchGame(gameId.Value, gameName);
		}

		private bool IsUnwantedGameId(Int64? gameId)
		{
			// if game Id is null, we say it is not unwanted

			if (gameId is Int64 id)
			{
				return twitchOptionsMonitor.CurrentValue.UnwantedGameIds.Contains(id);
			}
			else
			{
				return false;
			}
		}

		private async ValueTask<(HttpStatusCode, string)> RequestGraphQlDataAsync(IEnumerable<IStream> streams, CancellationToken cancellationToken)
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

			if (twitchOptionsMonitor.CurrentValue.GraphQlApiUri is Uri apiUri)
			{
				(HttpStatusCode, string) result = (HttpStatusCode.Unused, string.Empty);

				using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Twitch))
				{
					result = await HttpClientHelpers.GetStringAsync(client, apiUri, ConfigureRequest, cancellationToken).ConfigureAwait(false);
				}

				return result;
			}
			else
			{
				throw new TwitchException("GraphQl API was null");
			}
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
