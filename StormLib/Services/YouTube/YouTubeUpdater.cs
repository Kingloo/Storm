using System;
using System.Collections.Frozen;
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
		// 'CultureInfo.IetfLanguageTag's
		private const string EnglishAmerican = "en-US";
		private const string EnglishBritish = "en-GB";
		private const string FrenchFrance = "fr-FR";
		
		private static readonly FrozenDictionary<string, string> liveMarkers = new Dictionary<string, string>
		{
			{ EnglishBritish, "\"text\":\"LIVE\"" },
			{ FrenchFrance, "\"text\":\"EN DIRECT\"" }
		}
		.ToFrozenDictionary();
		
		private static readonly FrozenDictionary<string, string> upcomingMarkers = new Dictionary<string, string>
		{
			{ EnglishBritish, "\"text\":\"Upcoming\"" },
			{ FrenchFrance, "\"text\":\"À venir\"" }
		}
		.ToFrozenDictionary();
		
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

			CultureInfo cultureInfo = DetermineCulture(rawJson);

			string newDisplayName = GetDisplayName(json) is string { Length: > 0 } displayName
				? displayName
				: stream.Link.AbsoluteUri;

			Status newStatus = GetStatus(liveNode, cultureInfo, rawJson);

			int? newViewersCount = newStatus switch
			{
				Status.Public => GetViewers(rawJson, cultureInfo),
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

		private static CultureInfo DetermineCulture(string rawJson)
		{
			const string accessibilityTextEnglish = "\"accessibilityText\":\"Watch Later\"";
			const string accessibilityTextFrench = "\"accessibilityText\":\"À regarder plus tard\"";

			if (rawJson.Contains(accessibilityTextEnglish, StringComparison.Ordinal))
			{
				return CultureInfo.CreateSpecificCulture(EnglishBritish);
			}

			if (rawJson.Contains(accessibilityTextFrench, StringComparison.Ordinal))
			{
				return CultureInfo.CreateSpecificCulture(FrenchFrance);
			}

			// .NET 9 or greater: SearchValues can do this faster
			
			return CultureInfo.CreateSpecificCulture(EnglishAmerican);
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

		private static Status GetStatus(JsonNode? liveNode, CultureInfo cultureInfo, string rawJson)
		{
			if (liveNode is not null)
			{
				return Status.Public;
			}

			bool containsLiveMarker = liveMarkers.TryGetValue(cultureInfo.IetfLanguageTag, out string? liveMarker)
				&& rawJson.Contains(liveMarker, StringComparison.Ordinal);

			if (containsLiveMarker)
			{
				return Status.Public;
			}

			bool containsUpcomingMarker = upcomingMarkers.TryGetValue(cultureInfo.IetfLanguageTag, out string? upcomingMarker)
				&& rawJson.Contains(upcomingMarker, StringComparison.Ordinal);

			if (containsUpcomingMarker)
			{
				return Status.LiveSoon;
			}

			return Status.Offline;

			// .NET 9 or greater: use SearchValues for searching for multiple strings within a string
		}

		private static string? GetDisplayName(JsonNode json)
		{
			return (string?)json["header"]?["pageHeaderRenderer"]?["pageTitle"];
		}

		private static int? GetViewers(string text, CultureInfo cultureInfo)
		{
			/*

			Some examples of the viewers text:

			1 watching
			945 spectateurs
			1.1k watching
			1,1 k spectateurs
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

				string splitter = useNbsp ? nbsp : space;

				(string? numberStringValue, double magnitude) = cultureInfo.IetfLanguageTag switch
				{
					EnglishBritish => GetViewersFromEnglish(viewersText, splitter),
					FrenchFrance => GetViewersFromFrench(viewersText, splitter),
					_ => (null, double.NaN)
				};

				if (double.TryParse(numberStringValue, NumberStyles.AllowDecimalPoint, cultureInfo, out double result))
				{
					viewers = Convert.ToInt32(result * magnitude);
				}
			}

			return viewers;
		}

		private static (string, double) GetViewersFromEnglish(string viewersText, string splitter)
		{
			string[] segments = viewersText.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

			double magnitude = GetMagnitudeFromLetter(segments[1].Last());

			string numberStringValue = magnitude == 1
				? segments[0] // 873 watching
				: segments[0][..^1]; // 1.1k watching

			return (numberStringValue, magnitude);
		}

		private static (string?, double) GetViewersFromFrench(string viewersText, string splitter)
		{
			string[] segments = viewersText.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

			if (segments.Length == 0)
			{
				return (null, double.NaN);
			}
			
			double magnitude = GetMagnitudeFromLetter(viewersText.FirstOrDefault(FindMagnitudeLetter));

			string? numberStringValue = GetNumber(segments);

			return (numberStringValue.Trim(), magnitude);

			static string GetNumber(string[] segments)
			{
				if (segments.Length == 3
					&& segments[1].Length == 1
					&& Char.IsAsciiLetterLower(segments[1][0]))
				{
					// 1,3 k spectateurs

					return segments[0];
				}

				if (segments[0].EndsWith('k') || segments[0].EndsWith('m'))
				{
					// 1k spectateurs
					// 1,1k spectateurs

					return segments[0][..^1];
				}

				// 873 spectateurs
				
				return segments[0];
			}
		}

		private static double GetMagnitudeFromLetter(char c)
		{
			return c switch
			{
				'k' => 1_000,
				'm' => 1_000_000,
				_ => 1
			};
		}

		private static bool FindMagnitudeLetter(char arg)
		{
			return arg == 'k' || arg == 'm';
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
