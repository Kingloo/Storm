using System;
using System.Collections.Generic;
using System.Globalization;
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
using StormLib.Extensions;
using StormLib.Helpers;
using StormLib.Interfaces;

namespace StormLib.Services.YouTube
{
	public class YouTubeUpdater : IUpdater<YouTubeStream>
	{
		private static readonly string[] youTubeLiveMarkers = { "\"text\":\"LIVE\"", "\"text\":\"EN DIRECT\"" };
		
		private readonly ILogger<YouTubeUpdater> logger;
		private readonly IHttpClientFactory httpClientFactory;
		private readonly IOptionsMonitor<YouTubeOptions> youTubeOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public YouTubeUpdater(ILogger<YouTubeUpdater> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<YouTubeOptions> youTubeOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClientFactory);
			ArgumentNullException.ThrowIfNull(youTubeOptionsMonitor);

			this.logger = logger;
			this.httpClientFactory = httpClientFactory;
			this.youTubeOptionsMonitor = youTubeOptionsMonitor;
		}

		public Task<IReadOnlyList<Result<YouTubeStream>>> UpdateAsync(IReadOnlyList<YouTubeStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task<IReadOnlyList<Result<YouTubeStream>>> UpdateAsync(IReadOnlyList<YouTubeStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (streams.Count == 0)
			{
				return Array.Empty<Result<YouTubeStream>>();
			}

			if (streams.Count == 1)
			{
				Result<YouTubeStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);

				return new[] { singleResult };
			}

			return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
		}

		private async Task<IReadOnlyList<Result<YouTubeStream>>> UpdateManyAsync(IReadOnlyList<YouTubeStream> streams, CancellationToken cancellationToken)
		{
			List<Result<YouTubeStream>> results = new List<Result<YouTubeStream>>(capacity: streams.Count);

			for (int i = 0; i < streams.Count; i++)
			{
				YouTubeStream stream = streams[i];

				Result<YouTubeStream> result = await UpdateOneAsync(stream, cancellationToken).ConfigureAwait(false);

				results.Add(result);

				if (i < streams.Count - 1)
				{
					TimeSpan delay = GetManyUpdateDelay(streams.Count);

					logger.LogTrace("waiting for {Time} ms to update '{Stream}'", delay.TotalMilliseconds, streams[i + 1].Name);

					await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
				}
			}

			return results.AsReadOnly();
		}

		private async Task<Result<YouTubeStream>> UpdateOneAsync(YouTubeStream stream, CancellationToken cancellationToken)
		{
			logger.LogDebug("update '{DisplayName}'", stream.DisplayName);

			Uri uri = new Uri($"{stream.Link.AbsoluteUri}/streams?ucbcb=1", UriKind.Absolute);

			HttpStatusCode? statusCode = null;
			string text = string.Empty;

			static void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				requestMessage.Version = HttpVersion.Version20;
				requestMessage.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
			}

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.YouTube))
			{
				(statusCode, text) = await Helpers.HttpClientHelpers.GetStringAsync(client, uri, ConfigureRequest, cancellationToken).ConfigureAwait(false);
			}

			if (statusCode != HttpStatusCode.OK)
			{
				return HttpStatusNotOkResult(stream, statusCode);
			}

			string? rawJson = GetRawJson(text);

			if (rawJson is null)
			{
				return FailedToExtractRawJsonResult(stream, statusCode);
			}

			JsonNode? json = GetJson(rawJson);

			if (json is null)
			{
				return FailedToParseJsonResult(stream, statusCode);
			}

			return SuccessResult(stream, statusCode, json, rawJson);
		}

		private static Result<YouTubeStream> HttpStatusNotOkResult(YouTubeStream stream, HttpStatusCode? statusCode)
		{
			return new Result<YouTubeStream>(stream)
			{
				Action = static (YouTubeStream y) =>
				{
					y.Status = Status.Problem;
					y.ViewersCount = null;
				},
				StatusCode = statusCode,
				Message = $"status code was {HttpStatusCodeHelpers.FormatStatusCode(statusCode)}"
			};
		}

		private static Result<YouTubeStream> FailedToExtractRawJsonResult(YouTubeStream stream, HttpStatusCode? statusCode)
		{
			return new Result<YouTubeStream>(stream)
			{
				Action = (YouTubeStream y) =>
				{
					y.Status = Status.Problem;
					y.ViewersCount = null;
				},
				StatusCode = statusCode,
				Message = "failed to extract JSON from webpage"
			};
		}

		private static Result<YouTubeStream> FailedToParseJsonResult(YouTubeStream stream, HttpStatusCode? statusCode)
		{
			return new Result<YouTubeStream>(stream)
			{
				Action = (YouTubeStream y) =>
				{
					y.Status = Status.Problem;
					y.ViewersCount = null;
				},
				StatusCode = statusCode,
				Message = "JSON parsing failed"
			};
		}

		private static Result<YouTubeStream> SuccessResult(YouTubeStream stream, HttpStatusCode? statusCode, JsonNode json, string rawJson)
		{
			List<JsonNode> upcomingNodes = EmptyList<JsonNode>.Empty;
			JsonNode? liveNode = null;

			JsonArray? tabContents = ExtractTabContents(json);

			if (tabContents is not null)
			{
				upcomingNodes = GetUpcomingNodes(tabContents);
				liveNode = GetLiveNode(tabContents);
			}

			string newDisplayName = GetDisplayName(json) is string { Length: > 0 } displayName
				? displayName
				: stream.Link.AbsoluteUri;

			Status newStatus = GetLiveStatus(liveNode, upcomingNodes.Count, rawJson);

			int? newViewersCount = newStatus switch
			{
				Status.Public => GetViewers(rawJson),
				_ => null
			};

			return new Result<YouTubeStream>(stream)
			{
				Action = (YouTubeStream y) =>
				{
					y.DisplayName = newDisplayName;
					y.Status = newStatus;
					y.ViewersCount = newViewersCount;
				},
				StatusCode = statusCode,
				Message = $"updated {newDisplayName}: {newStatus}"
			};
		}

		private static string? GetRawJson(string text)
		{
			const string beginning = "var ytInitialData = ";
			const string ending = ";</script>";

			return text.FindBetween(beginning, ending).FirstOrDefault();
		}

		private static JsonNode? GetJson(string? rawJson)
		{
			return rawJson is string { Length: > 0 } && JsonHelpers.TryParse(rawJson, out JsonNode? jsonNode)
				? jsonNode
				: null;
		}

		private static JsonArray? ExtractTabContents(JsonNode json)
		{
			JsonArray? tabs = (JsonArray?)json["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"];
			
			JsonNode? firstTabWithContent = tabs
				?.FirstOrDefault(static each =>
					each?["tabRenderer"]?["content"] is JsonNode withContent && withContent.GetValueKind() == JsonValueKind.Object
				);
			
			return (JsonArray?)firstTabWithContent?["tabRenderer"]?["content"]?["richGridRenderer"]?["contents"];
		}

		private static List<JsonNode> GetUpcomingNodes(JsonArray tabContents)
		{
			return tabContents
				.Where(static each =>
					each?["richItemRenderer"]?["content"]?["videoRenderer"]?["upcomingEventData"] is JsonNode eachNode && eachNode.GetValueKind() == JsonValueKind.Object
				)
				.Cast<JsonNode>()
				.ToList()
			?? EmptyList<JsonNode>.Empty;
		}

		private static JsonNode? GetLiveNode(JsonArray tabContents)
		{
			return tabContents.FirstOrDefault(static (JsonNode? each) =>
			{
				JsonNode? videoRenderer = each?["richItemRenderer"]?["content"]?["videoRenderer"];
				
				JsonArray? thumbnailOverlays = (JsonArray?)videoRenderer?["thumbnailOverlays"];

				JsonNode? iconTypeNode = thumbnailOverlays
					?.FirstOrDefault(static each => each?["thumbnailOverlayTimeStatusRenderer"]?["icon"]?["iconType"] is JsonNode iconType && iconType.GetValueKind() == JsonValueKind.String);

				return String.Equals((string?)iconTypeNode?["thumbnailOverlayTimeStatusRenderer"]?["icon"]?["iconType"], "LIVE", StringComparison.OrdinalIgnoreCase);
			},
			null);
		}

		private static Status GetLiveStatus(JsonNode? liveNode, int upcomingNodesCount, string rawJson)
		{
			if (liveNode is not null)
			{
				return Status.Public;
			}

			bool containsAnyLiveMarker = youTubeLiveMarkers.Any(marker => rawJson.Contains(marker, StringComparison.Ordinal));
			// .NET 9 or greater: use SearchValues for searching for multiple string within a string
			
			return containsAnyLiveMarker
				? Status.Public
				: upcomingNodesCount > 0
					? Status.LiveSoon
					: Status.Offline;
		}

		private static string? GetDisplayName(JsonNode json)
		{
			return (string?)json["header"]?["pageHeaderRenderer"]?["pageTitle"];
		}

		private static int? GetViewers(string text)
		{
			/*

			Some examples of the viewers text:

			1 watching
			945 spectateurs
			1.1k watching
			1,1k spectateurs
			14k spectateurs

			French decimal separator is ',' (comma)
			English decimal separator is '.' (dot)

			the space is an nbsp UTF-8 00A0 (sometimes C2 A0)

			Limitation: for viewer counts > 1000, we can only get approximate numbers
			e.g. 1.1k turns into 1100 viewers

			*/
			
			int? viewers = null;

			const string beginning = "\"text\":{\"content\":\"";
			const string ending = "\"}";
			const string nbsp = "\u00A0";
			const string space = " ";
			
			if (text.FindBetween(beginning, ending).FirstOrDefault() is string { Length: > 0 } viewersText)
			{
				bool useNbsp = viewersText.Contains(nbsp, StringComparison.OrdinalIgnoreCase);

				string[] segments = viewersText.Split(useNbsp ? nbsp : space, StringSplitOptions.RemoveEmptyEntries);

				if (segments.Length != 2)
				{
					return null;
				}
				
				double magnitude = segments[1].Last() switch
				{
					'k' => 1_000,
					'm' => 1_000_000, // never seen live stream with more than 1 million viewers, presuming it would use 'm'
					_ => 1
				};

				CultureInfo culture = String.Equals(segments[1], "watching", StringComparison.OrdinalIgnoreCase)
					? CultureInfo.CreateSpecificCulture("en") // uses '.' (dot) as decimal separator
					: CultureInfo.CreateSpecificCulture("fr"); // uses ',' (comma) as decimal separator

				string number = magnitude == 1
					? segments[0]
					: segments[0][..^1];
				// removes the 'k' or 'm' at the end of the number

				if (double.TryParse(number, NumberStyles.AllowDecimalPoint, culture, out double result))
				{
					viewers = Convert.ToInt32(result * magnitude);
				}
			}

			return viewers;
		}

		private static TimeSpan GetManyUpdateDelay(int totalToUpdate)
		{
			(int minimumInc, int maximumEx) = totalToUpdate switch
			{
				<= 5 => (100, 500),
				<= 10 => (500, 1000),
				> 10 => (1000, 2000)
			};

			int delayMilliseconds = System.Security.Cryptography.RandomNumberGenerator.GetInt32(minimumInc, maximumEx);

			return TimeSpan.FromMilliseconds(delayMilliseconds);
		}
	}
}
