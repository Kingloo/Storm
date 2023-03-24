using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StormLib.Common;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services
{
	public class KickService : IService, IDisposable
	{
		private const string api = "https://kick.com/api/v1";

		private readonly IDownload download;

		public Type HandlesStreamType { get => typeof(KickStream); }

		public KickService(IDownload download)
		{
			ArgumentNullException.ThrowIfNull(download);

			this.download = download;
		}

		public async Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext)
		{
			ArgumentNullException.ThrowIfNull(stream);

			Uri apiEndpointForStream = new Uri($"{api}/channels/{stream.Name}", UriKind.Absolute);

			static void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				requestMessage.Headers.Add(UserAgents.HeaderName, UserAgents.Firefox_111_Windows);
				requestMessage.Headers.Host = "kick.com";
				requestMessage.Method = HttpMethod.Get;
				requestMessage.Version = HttpVersion.Version20;
			};

			(HttpStatusCode statusCode, string text) = await download.StringAsync(apiEndpointForStream, ConfigureRequest).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;

				return Result.WebFailure;
			}

			if (!JsonHelpers.TryParse(text, out JObject? json))
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;
				
				return Result.ParsingJsonFailed;
			}

			if (json?["user"]?["username"] is JToken displayNameToken)
			{
				if (displayNameToken.Type is JTokenType.String)
				{
					if ((string?)displayNameToken is string displayNameValue)
					{
						stream.DisplayName = displayNameValue;
					}
				}
			}

			if (json?["livestream"]?.HasValues ?? false)
			{
				stream.Status = Status.Public;

				if (json?["livestream"]?["viewer_count"] is JToken viewerCountToken)
				{
					if (viewerCountToken.Type is JTokenType.Integer)
					{
						stream.ViewersCount = (int)viewerCountToken;
					}
				}
			}
			else
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;
			}

			return Result.Success;
		}

		public async Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext)
		{
			ArgumentNullException.ThrowIfNull(streams);

			IList<IStream> enumeratedStreams = streams.ToList();

			if (!enumeratedStreams.Any())
			{
				return Result.NothingToDo;
			}

			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (IStream each in enumeratedStreams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateAsync(each, preserveSynchronizationContext));

				updateTasks.Add(updateTask);
			}

			Result[] results = await Task.WhenAll(updateTasks).ConfigureAwait(preserveSynchronizationContext);

			return results.OrderByDescending(static r => r).First();
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
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}