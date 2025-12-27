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

			return new Result<YouTubeStream>(stream)
			{
				Action = (YouTubeStream y) =>
				{
					JsonNode? json = GetJson(text);
					JsonArray? tabs = (JsonArray?)json?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"];
					JsonNode? firstTabWithContent = (JsonNode?)tabs?.FirstOrDefault(each => each?["tabRenderer"]?["content"] is JsonNode withContent && withContent.GetValueKind() == JsonValueKind.Object);
					JsonArray? tabContents = (JsonArray?)firstTabWithContent?["tabRenderer"]?["content"]?["richGridRenderer"]?["contents"];

					List<JsonNode?> allUpcoming = tabContents
						?.Where(each => each?["richItemRenderer"]?["content"]?["videoRenderer"]?["upcomingEventData"] is JsonNode eachNode && eachNode.GetValueKind() == JsonValueKind.Object)
						.ToList()
						?? new List<JsonNode?>(capacity: 0);

					JsonNode? live = tabContents?.SingleOrDefault((JsonNode? each) =>
					{
						JsonNode? videoRenderer = each?["richItemRenderer"]?["content"]?["videoRenderer"];
						JsonArray? thumbnailOverlays = (JsonArray?)videoRenderer?["thumbnailOverlays"];

						JsonNode? iconTypeNode = thumbnailOverlays
							?.FirstOrDefault(each => each?["thumbnailOverlayTimeStatusRenderer"]?["icon"]?["iconType"] is JsonNode iconType && iconType.GetValueKind() == JsonValueKind.String);

						return String.Equals((string?)iconTypeNode?["thumbnailOverlayTimeStatusRenderer"]?["icon"]?["iconType"], "LIVE", StringComparison.OrdinalIgnoreCase);
					});

					y.DisplayName = GetDisplayName(json) ?? stream.Link.AbsoluteUri;

					y.Status = (live is not null) switch
					{
						true => Status.Public,
						false => (allUpcoming is not null && allUpcoming.Count > 0) ? Status.LiveSoon : Status.Offline
					};

					y.ViewersCount = live is not null
						? GetViewers(live)
						: GetViewers(allUpcoming?.LastOrDefault());
				},
				StatusCode = statusCode
			};
		}

		private static JsonNode? GetJson(string text)
		{
			const string beginning = "var ytInitialData = ";
			const string ending = ";</script>";

			string rawJson = text.FindBetween(beginning, ending).FirstOrDefault() ?? string.Empty;

			return JsonHelpers.TryParse(rawJson, out JsonNode? jsonNode) ? jsonNode : null;
		}

		private static string? GetDisplayName(JsonNode? json)
		{
			return (string?)json?["header"]?["pageHeaderRenderer"]?["pageTitle"];
		}

		private static int? GetViewers(JsonNode? node)
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

		private static string GetOnlyDigits(string text)
		{
			return new StringBuilder()
				.Append(text.Trim().Where(static c => Char.IsDigit(c)).ToArray())
				.ToString();
		}
	}
}
