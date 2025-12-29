using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib.Extensions;
using StormLib.Helpers;
using StormLib.Interfaces;

namespace StormLib.Services.YouTube
{
	public class YouTubeUpdater : IUpdater<YouTubeStream>
	{
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

			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.YouTube))
			{
				(statusCode, text) = await Helpers.HttpClientHelpers.GetStringAsync(client, uri, cancellationToken).ConfigureAwait(false);
			}

			if (statusCode != HttpStatusCode.OK)
			{
				return new Result<YouTubeStream>(stream)
				{
					Action = static (YouTubeStream y) =>
					{
						y.Status = Status.Problem;
						y.ViewersCount = null;
					},
					StatusCode = statusCode
				};
			}

			JsonNode? json = GetJson(text, stream.Link.AbsoluteUri);

			if (json is null)
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

			return new Result<YouTubeStream>(stream)
			{
				Action = (YouTubeStream y) =>
				{
					List<JsonNode> upcomingNodes = new List<JsonNode>(capacity: 0);
					JsonNode? liveNode = null;

					JsonArray? tabContents = ExtractTabContents(json);

					if (tabContents is not null)
					{
						upcomingNodes = GetUpcomingNodes(tabContents);
						liveNode = GetLiveNode(tabContents);
					}
					
					y.DisplayName = GetDisplayName(json) is string { Length: > 0 } displayName
						? displayName
						: stream.Link.AbsoluteUri;

					y.Status = liveNode is not null
						? Status.Public
						: (upcomingNodes.Count > 0) ? Status.LiveSoon : Status.Offline;

					y.ViewersCount = liveNode is not null
						? GetViewers(liveNode)
						: upcomingNodes.Count > 0
							? GetViewers(upcomingNodes.Last())
							: null;
				},
				StatusCode = statusCode
			};
		}

		private JsonNode? GetJson(string text, string streamUri)
		{
			const string beginning = "var ytInitialData = ";
			const string ending = ";</script>";

			string rawJson = text.FindBetween(beginning, ending).FirstOrDefault() ?? string.Empty;

			if (JsonHelpers.TryParse(rawJson, out JsonNode? jsonNode))
			{
				return jsonNode;
			}
			else
			{
				logger.LogWarning("YouTube: parsing JSON failed for '{StreamUri}'", streamUri);

				return null;
			}
		}

		private static JsonArray? ExtractTabContents(JsonNode json)
		{
			JsonArray? tabs = (JsonArray?)json["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"];
			JsonNode? firstTabWithContent = tabs?.FirstOrDefault(static each => each?["tabRenderer"]?["content"] is JsonNode withContent && withContent.GetValueKind() == JsonValueKind.Object);
			return (JsonArray?)firstTabWithContent?["tabRenderer"]?["content"]?["richGridRenderer"]?["contents"];
		}

		private static List<JsonNode> GetUpcomingNodes(JsonArray tabContents)
		{
			return tabContents
				.Where(static each => each?["richItemRenderer"]?["content"]?["videoRenderer"]?["upcomingEventData"] is JsonNode eachNode && eachNode.GetValueKind() == JsonValueKind.Object)
				.Cast<JsonNode>()
				.ToList()
			?? new List<JsonNode>(capacity: 0);
		}

		private static JsonNode? GetLiveNode(JsonArray tabContents)
		{
			return tabContents.SingleOrDefault(static (JsonNode? each) =>
			{
				JsonNode? videoRenderer = each?["richItemRenderer"]?["content"]?["videoRenderer"];
				JsonArray? thumbnailOverlays = (JsonArray?)videoRenderer?["thumbnailOverlays"];

				JsonNode? iconTypeNode = thumbnailOverlays
					?.FirstOrDefault(each => each?["thumbnailOverlayTimeStatusRenderer"]?["icon"]?["iconType"] is JsonNode iconType && iconType.GetValueKind() == JsonValueKind.String);

				return String.Equals((string?)iconTypeNode?["thumbnailOverlayTimeStatusRenderer"]?["icon"]?["iconType"], "LIVE", StringComparison.OrdinalIgnoreCase);
			},
			null);
		}

		private static string? GetDisplayName(JsonNode json)
		{
			return (string?)json["header"]?["pageHeaderRenderer"]?["pageTitle"];
		}

		private static int? GetViewers(JsonNode node)
		{
			JsonArray? runs = (JsonArray?)node?["richItemRenderer"]?["content"]?["videoRenderer"]?["viewCountText"]?["runs"];

			string? number = (string?)runs?.FirstOrDefault()?["text"];

			if (number is null)
			{
				return null;
			}

			string onlyDigits = GetOnlyDigits(number);

			return int.TryParse(onlyDigits, out int viewers) ? viewers : null;
		}

		private static string GetOnlyDigits(string text)
		{
			return new StringBuilder()
				.Append(text.Trim().Where(static c => Char.IsDigit(c)).ToArray())
				.ToString();
		}

		private static TimeSpan GetManyUpdateDelay(int totalToUpdate)
		{
			(int minimum, int maximum) = totalToUpdate switch
			{
				<= 5 => (100, 500),
				<= 10 => (500, 1000),
				> 10 => (1000, 2000)
			};

			int delayMilliseconds = System.Security.Cryptography.RandomNumberGenerator.GetInt32(minimum, maximum);

			return TimeSpan.FromMilliseconds(delayMilliseconds);
		}
	}
}
