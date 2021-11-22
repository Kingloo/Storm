using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StormLib.Common;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
	public class ChaturbateService : IService, IDisposable
	{
		private const string bannedMarker = "has been banned";
		private const string roomStatus = "room_status";
		private const string publicStatus = "public";
		private const string offlineStatus = "offline";
		private const string awayStatus = "away";
		private const string privateStatus = "private";

		private readonly IDownload download;

		public Type HandlesStreamType { get; } = typeof(ChaturbateStream);

		public ChaturbateService(IDownload download)
		{
			this.download = download;
		}

		public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
		{
			(HttpStatusCode status, string text) = await download.StringAsync(stream.Link).ConfigureAwait(preserveSynchronizationContext);

			if (status != HttpStatusCode.OK)
			{
				return Result.WebFailure;
			}

			if (text.Contains(bannedMarker))
			{
				stream.Status = Status.Banned;
				return Result.Success;
			}

			int index = text.IndexOf(roomStatus, StringComparison.OrdinalIgnoreCase);

			if (index < 0)
			{
				return Result.Failure;
			}

			if (text.Length < index + 100)
			{
				return Result.Failure;
			}

			string searchRadius = text.Substring(index, 100);

			if (searchRadius.Contains(publicStatus))
			{
				stream.Status = Status.Public;
			}
			else if (searchRadius.Contains(offlineStatus)
				|| searchRadius.Contains(awayStatus))
			{
				stream.Status = Status.Offline;
			}
			else if (searchRadius.Contains(privateStatus))
			{
				stream.Status = Status.Private;
			}
			else
			{
				stream.Status = Status.Unknown;
			}

			return Result.Success;
		}

		public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
		{
			if (!streams.Any()) { return Result.NothingToDo; }

			List<Task<Result>> tasks = new List<Task<Result>>();

			foreach (IStream stream in streams)
			{
				Task<Result> task = Task.Run(() => UpdateAsync(stream, preserveSynchronizationContext));

				tasks.Add(task);
			}

			Result[] results = await Task.WhenAll(tasks).ConfigureAwait(preserveSynchronizationContext);

			return results.OrderByDescending(r => r).First();
		}

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					download.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
