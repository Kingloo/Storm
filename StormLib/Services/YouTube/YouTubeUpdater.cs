using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib.Extensions;
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

			if (!streams.Any())
			{
				return Array.Empty<Result<YouTubeStream>>();
			}

			if (streams.Count == 1)
			{
				Result<YouTubeStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);

				return new[] { singleResult };
			}
			else
			{
				return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
			}
		}

		private async Task<Result<YouTubeStream>> UpdateOneAsync(YouTubeStream stream, CancellationToken cancellationToken)
		{
			logger.LogDebug("update '{}'", stream.DisplayName);

			Uri uri = new Uri($"{stream.Link.AbsoluteUri}?ucbcb=1", UriKind.Absolute);

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
					y.DisplayName = SetDisplayName(text, stream.Name);
					y.Status = SetStatus(text, youTubeOptionsMonitor.CurrentValue);
					y.ViewersCount = SetViewers(text);
				},
				StatusCode = statusCode
			};
		}

		private Task<Result<YouTubeStream>[]> UpdateManyAsync(IReadOnlyList<YouTubeStream> streams, CancellationToken cancellationToken)
		{
			List<Task<Result<YouTubeStream>>> updateTasks = new List<Task<Result<YouTubeStream>>>();

			foreach (YouTubeStream each in streams)
			{
				Task<Result<YouTubeStream>> updateTask = Task.Run(() => UpdateOneAsync(each, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}

		private static string SetDisplayName(string text, string fallback)
		{
			// "twitter:title" content="Eris In Progress">
			// "twitter:title" content="
			// ">
			// Eris In Progress

			const string beginning = "\"twitter:title\" content=\"";
			const string ending = "\">";

			return text.FindBetween(beginning, ending).FirstOrDefault() ?? fallback;
		}

		private static Status SetStatus(string text, YouTubeOptions youTubeOptions)
		{
			return text.Contains(youTubeOptions.LiveMarker, StringComparison.OrdinalIgnoreCase)
				? Status.Public
				: Status.Offline;
		}

		private static int? SetViewers(string text)
		{
			const string beginning = "viewCountText\":{\"runs\":[{\"text\":\"";
			const string ending = "\"}";

			string? viewersText = text.FindBetween(beginning, ending).FirstOrDefault();

			if (viewersText is null)
			{
				return null;
			}

			string viewersTextDigitsOnly = GetOnlyDigits(viewersText);

			return Int32.TryParse(viewersTextDigitsOnly, out int result)
				? result
				: null;
		}

		private static string GetOnlyDigits(string text)
		{
			return new StringBuilder()
				.Append(text.Trim().Where(static c => Char.IsDigit(c)).ToArray())
				.ToString();
		}
	}
}
