using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StormLib.Interfaces;

namespace StormLib.Services.Chaturbate
{
	public class ChaturbateUpdater : IUpdater<ChaturbateStream>
	{
		private const string bannedMarker = "has been banned";
		private const string roomStatus = "room_status";
		private const string publicStatus = "public";
		private const string offlineStatus = "offline";
		private const string awayStatus = "away";
		private const string privateStatus = "private";

		public UpdaterType UpdaterType { get; } = UpdaterType.One;

		private readonly ILogger<ChaturbateUpdater> logger;
		private readonly IHttpClientFactory httpClientFactory;

		public ChaturbateUpdater(ILogger<ChaturbateUpdater> logger, IHttpClientFactory httpClientFactory)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(httpClientFactory);

			this.logger = logger;
			this.httpClientFactory = httpClientFactory;
		}

		public Task<IReadOnlyList<Result<ChaturbateStream>>> UpdateAsync(IReadOnlyList<ChaturbateStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task<IReadOnlyList<Result<ChaturbateStream>>> UpdateAsync(IReadOnlyList<ChaturbateStream> streams, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			if (streams.Count == 0)
			{
				return Array.Empty<Result<ChaturbateStream>>();
			}

			if (streams.Count == 1)
			{
				Result<ChaturbateStream> singleResult = await UpdateOneAsync(streams[0], cancellationToken).ConfigureAwait(false);

				return new[] { singleResult };
			}
			
			return await UpdateManyAsync(streams, cancellationToken).ConfigureAwait(false);
		}

		private async Task<Result<ChaturbateStream>> UpdateOneAsync(ChaturbateStream stream, CancellationToken cancellationToken)
		{
			HttpStatusCode statusCode = HttpStatusCode.Unused;
			string text = string.Empty;

			Status newStatus = Status.Unknown;
			int? newViewersCount = null;

			void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				requestMessage.Headers.Host = stream.Link.DnsSafeHost;
				requestMessage.Headers.Referrer = stream.Link;
			}

			using (HttpClient client = httpClientFactory.CreateClient(HttpClientNames.Chaturbate))
			{
				Uri channelRelativeUri = new Uri($"{stream.Name}", UriKind.Relative);

				(statusCode, text) = await Helpers.HttpClientHelpers.GetStringAsync(client, channelRelativeUri, ConfigureRequest, cancellationToken).ConfigureAwait(false);
			}

			if (statusCode != HttpStatusCode.OK)
			{
				return new Result<ChaturbateStream>(stream)
				{
					Action = static (ChaturbateStream c) =>
					{
						c.Status = Status.Problem;
						c.ViewersCount = null;
					},
					StatusCode = statusCode
				};
			}

			if (text.Contains(bannedMarker, StringComparison.OrdinalIgnoreCase))
			{
				return new Result<ChaturbateStream>(stream)
				{
					Action = static (ChaturbateStream c) =>
					{
						c.Status = Status.Banned;
						c.ViewersCount = null;
					},
					StatusCode = statusCode
				};
			}

			int roomStatusIndex = text.IndexOf(roomStatus, StringComparison.OrdinalIgnoreCase);

			if (roomStatusIndex < 0)
			{
				return new Result<ChaturbateStream>(stream)
				{
					Action = static (ChaturbateStream c) =>
					{
						c.Status = Status.Problem;
						c.ViewersCount = null;
					},
					Message = $"text did not contain room status: '{roomStatus}'",
					StatusCode = statusCode
				};
			}

			if (text.Length < roomStatusIndex + 100)
			{
				return new Result<ChaturbateStream>(stream)
				{
					Action = static (ChaturbateStream c) =>
					{
						c.Status = Status.Problem;
						c.ViewersCount = null;
					},
					Message = $"there was not 100 characters after room status index, there were actually {text.Length - roomStatusIndex} characters",
					StatusCode = statusCode
				};
			}

			string searchRadius = text.Substring(roomStatusIndex, 100);

			if (searchRadius.Contains(publicStatus, StringComparison.OrdinalIgnoreCase))
			{
				newStatus = Status.Public;
			}
			else if (searchRadius.Contains(offlineStatus, StringComparison.OrdinalIgnoreCase)
				|| searchRadius.Contains(awayStatus, StringComparison.OrdinalIgnoreCase))
			{
				newStatus = Status.Offline;
			}
			else if (searchRadius.Contains(privateStatus, StringComparison.OrdinalIgnoreCase))
			{
				newStatus = Status.Private;
			}
			else
			{
				newStatus = Status.Unknown;
			}

			return new Result<ChaturbateStream>(stream)
			{
				Action = (ChaturbateStream c) =>
				{
					c.Status = newStatus;
					c.ViewersCount = newViewersCount;
				},
				StatusCode = statusCode
			};
		}

		private async Task<Result<ChaturbateStream>[]> UpdateManyAsync(IReadOnlyList<ChaturbateStream> streams, CancellationToken cancellationToken)
		{
			// Chaturbate doesn't like it if you hit them too often,
			// connections time out for minutes without connecting

			List<Result<ChaturbateStream>> updateResults = new List<Result<ChaturbateStream>>(capacity: streams.Count);

			foreach (ChaturbateStream stream in streams)
			{
				Result<ChaturbateStream> result;

				try
				{
					result = await UpdateOneAsync(stream, cancellationToken).ConfigureAwait(false);
				}
				catch (TaskCanceledException ex)
				{
					logger.LogError(ex, "{Message}", ex.Message);

					result = new Result<ChaturbateStream>(stream)
					{
						Action = (ChaturbateStream c) =>
						{
							c.Status = Status.Problem;
							c.ViewersCount = null;
						},
						StatusCode = HttpStatusCode.RequestTimeout
					};
				}

				updateResults.Add(result);

				int randomisedUpdateDelaySeconds = System.Security.Cryptography.RandomNumberGenerator.GetInt32(5, 15);

				await Task.Delay(TimeSpan.FromSeconds(randomisedUpdateDelaySeconds), cancellationToken).ConfigureAwait(false);
			}

			return updateResults.ToArray();
		}
	}
}
