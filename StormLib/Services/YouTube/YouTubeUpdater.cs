using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Extensions;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services.YouTube
{
	public class YouTubeUpdater : IUpdater<YouTubeStream>
	{
		private readonly ILogger<YouTubeUpdater> logger;
		private readonly IOptionsMonitor<YouTubeOptions> youTubeOptionsMonitor;

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		public YouTubeUpdater(ILogger<YouTubeUpdater> logger, IOptionsMonitor<YouTubeOptions> youTubeOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(youTubeOptionsMonitor);

			this.logger = logger;
			this.youTubeOptionsMonitor = youTubeOptionsMonitor;
		}

		public Task<Result[]> UpdateAsync(IList<YouTubeStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);

		public Task<Result[]> UpdateAsync(IList<YouTubeStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);

		public Task<Result[]> UpdateAsync(IList<YouTubeStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);

		public async Task<Result[]> UpdateAsync(IList<YouTubeStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (!streams.Any())
			{
				return Array.Empty<Result>();
			}

			if (streams.Count == 1)
			{
				Result singleResult = await UpdateOneAsync(streams[0], preserveSynchronizationContext, cancellationToken).ConfigureAwait(preserveSynchronizationContext);

				return new [] { singleResult };
			}
			else
			{
				return await UpdateManyAsync(streams, preserveSynchronizationContext, cancellationToken).ConfigureAwait(preserveSynchronizationContext);
			}
		}

		private async Task<Result> UpdateOneAsync(YouTubeStream stream, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			(HttpStatusCode statusCode, string text) = await download.StringAsync(new Uri($"{stream.Link.AbsoluteUri}?ucbcb=1")).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode);
			}

			stream.DisplayName = SetDisplayName(text, stream.Name);
            stream.Status = SetStatus(text, youTubeOptionsMonitor.CurrentValue);
            stream.ViewersCount = SetViewers(text);

			return new Result(UpdaterType, statusCode);
		}

		private Task<Result[]> UpdateManyAsync(IList<YouTubeStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (YouTubeStream each in streams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateOneAsync(each, preserveSynchronizationContext, cancellationToken));

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
            return text.Contains(youTubeOptions.LiveMarker, StringComparison.OrdinalIgnoreCase) switch
			{
				true => Status.Public,
				false => Status.Offline
			};
        }

        private static Nullable<int> SetViewers(string text)
        {
            const string beginning = "viewCountText\":{\"runs\":[{\"text\":\"";
            const string ending = "\"}";

            string? viewersText = text.FindBetween(beginning, ending).FirstOrDefault();

			if (viewersText is null)
			{
				return null;
			}

			string viewersTextDigitsOnly = GetOnlyDigits(viewersText);

            if (Int32.TryParse(viewersTextDigitsOnly, out int result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

		private static string GetOnlyDigits(string text)
		{
			return new StringBuilder()
				.Append(text.Trim().Where(static c => Char.IsDigit(c)).ToArray())
				.ToString();
		}
	}
}
