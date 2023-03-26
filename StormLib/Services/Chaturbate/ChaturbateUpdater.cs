using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Interfaces;
using StormLib.Streams;

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

		public ChaturbateUpdater(ILogger<ChaturbateUpdater> logger)
		{
			ArgumentNullException.ThrowIfNull(logger);

			this.logger = logger;
		}

		public Task<Result[]> UpdateAsync(IList<ChaturbateStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);
		
		public Task<Result[]> UpdateAsync(IList<ChaturbateStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);
		
		public Task<Result[]> UpdateAsync(IList<ChaturbateStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);

		public async Task<Result[]> UpdateAsync(IList<ChaturbateStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
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

		private async Task<Result> UpdateOneAsync(ChaturbateStream stream, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			(HttpStatusCode statusCode, string text) = await download.StringAsync(stream.Link).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode);
			}

			if (text.Contains(bannedMarker, StringComparison.OrdinalIgnoreCase))
			{
				stream.Status = Status.Banned;
				stream.ViewersCount = null;
				
				return new Result(UpdaterType, statusCode);
			}

			int roomStatusIndex = text.IndexOf(roomStatus, StringComparison.OrdinalIgnoreCase);

			if (roomStatusIndex < 0)
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode)
				{
					Message = $"text did not contain room status: '{roomStatus}'"
				};
			}

			if (text.Length < roomStatusIndex + 100)
			{
				stream.Status = Status.Problem;
				stream.ViewersCount = null;

				return new Result(UpdaterType, statusCode)
				{
					Message = $"there was not 100 characters after room status index, there were actually {text.Length - roomStatusIndex} characters"
				};
			}

			string searchRadius = text.Substring(roomStatusIndex, 100);

			if (searchRadius.Contains(publicStatus, StringComparison.OrdinalIgnoreCase))
			{
				stream.Status = Status.Public;
			}
			else if (searchRadius.Contains(offlineStatus, StringComparison.OrdinalIgnoreCase)
				|| searchRadius.Contains(awayStatus, StringComparison.OrdinalIgnoreCase))
			{
				stream.Status = Status.Offline;
			}
			else if (searchRadius.Contains(privateStatus, StringComparison.OrdinalIgnoreCase))
			{
				stream.Status = Status.Private;
			}
			else
			{
				stream.Status = Status.Unknown;
			}

			return new Result(UpdaterType, statusCode);
		}

		private Task<Result[]> UpdateManyAsync(IList<ChaturbateStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (ChaturbateStream each in streams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateOneAsync(each, preserveSynchronizationContext, cancellationToken));

				updateTasks.Add(updateTask);
			}

			return Task.WhenAll(updateTasks);
		}
	}
}
