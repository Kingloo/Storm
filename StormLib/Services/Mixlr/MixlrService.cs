using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
	public class MixlrService : IService, IDisposable
	{
		private readonly IDownload download;

		public Type HandlesStreamType { get; } = typeof(MixlrStream);

		public MixlrService(IDownload download)
		{
			this.download = download;
		}

		public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
		{
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
			
            UriBuilder apiCall = new UriBuilder
			{
				Host = "api.mixlr.com",
				Path = $"/users/{stream.Name}",
				Port = 443,
				Scheme = "https://"
			};

			(HttpStatusCode status, string text) = await download.StringAsync(apiCall.Uri).ConfigureAwait(preserveSynchronizationContext);

			if (status != HttpStatusCode.OK)
			{
				return Result.WebFailure;
			}

			if (!JsonHelpers.TryParse(text, out JObject? json))
			{
				return Result.ParsingJsonFailed;
			}

#nullable disable
			if (!json.HasValues)
			{
				return Result.Failure;
			}

			if (json.TryGetValue("username", out JToken usernameToken)
				&& json.TryGetValue("is_live", out JToken isLiveToken))
			{
				string username = (string)usernameToken;

				if (!String.IsNullOrWhiteSpace(username)
					&& (stream.DisplayName != username))
				{
					stream.DisplayName = username;
				}
#nullable enable

				stream.Status = (bool)isLiveToken ? Status.Public : Status.Offline;

				return Result.Success;
			}

			return Result.Failure;
		}

		public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
		{
			if (streams is null)
            {
                throw new ArgumentNullException(nameof(streams));
            }

			IList<IStream> enumeratedStreams = streams.ToList<IStream>();

            if (!enumeratedStreams.Any())
            {
                return Result.NothingToDo;
            }

			List<Task<Result>> tasks = new List<Task<Result>>();

			foreach (IStream stream in enumeratedStreams)
			{
				Task<Result> task = Task<Result>.Run(() => UpdateAsync(stream, preserveSynchronizationContext));

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
