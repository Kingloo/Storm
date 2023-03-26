using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Streams;

namespace StormLib.Services.Kick
{
	public class KickUpdater : IUpdater<KickStream>
	{
		private readonly ILogger<KickUpdater> logger;
		private readonly IOptionsMonitor<KickOptions> kickOptionsMonitor;
		private readonly IOptionsMonitor<StormOptions> stormOptionsMonitor;

		public KickUpdater(ILogger<KickUpdater> logger, IOptionsMonitor<KickOptions> kickOptionsMonitor, IOptionsMonitor<StormOptions> stormOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(kickOptionsMonitor);
			ArgumentNullException.ThrowIfNull(stormOptionsMonitor);

			this.logger = logger;
			this.kickOptionsMonitor = kickOptionsMonitor;
			this.stormOptionsMonitor = stormOptionsMonitor;
		}

		public Task<Result> UpdateAsync(IList<KickStream> streams)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, CancellationToken.None);

		public Task<Result> UpdateAsync(IList<KickStream> streams, bool preserveSynchronizationContext)
			=> UpdateAsync(streams, preserveSynchronizationContext, CancellationToken.None);

		public Task<Result> UpdateAsync(IList<KickStream> streams, CancellationToken cancellationToken)
			=> UpdateAsync(streams, preserveSynchronizationContext: false, cancellationToken);
		
		public Task<Result> UpdateAsync(IList<KickStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(streams);

			return streams.Count switch
			{
				0 => Task.FromResult(new Result()),
				1 => UpdateOneAsync(streams[0], preserveSynchronizationContext, cancellationToken),
				_ => UpdateManyAsync(streams, preserveSynchronizationContext, cancellationToken)
			};	
		}

		private async Task<Result> UpdateOneAsync(KickStream stream, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			Uri apiEndpointForStream = new Uri($"{kickOptionsMonitor.CurrentValue.ApiUri}/channels/{stream.Name}", UriKind.Absolute);

			void ConfigureRequest(HttpRequestMessage requestMessage)
			{
				foreach (KeyValuePair<HeaderName, HeaderValue> kvp in stormOptionsMonitor.CurrentValue.CommonHeaders)
				{
					requestMessage.Headers.Add(kvp.Key.Value, kvp.Value.Value);
				}

				foreach (KeyValuePair<HeaderName, HeaderValue> kvp in kickOptionsMonitor.CurrentValue.Headers)
				{
					requestMessage.Headers.Add(kvp.Key.Value, kvp.Value.Value);
				}

				requestMessage.Headers.Host = "kick.com";
				requestMessage.Method = HttpMethod.Get;
				requestMessage.Version = HttpVersion.Version20;
			};

			(HttpStatusCode statusCode, string text) = await download.StringAsync(apiEndpointForStream, ConfigureRequest).ConfigureAwait(preserveSynchronizationContext);

			if (statusCode != HttpStatusCode.OK)
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;

				return new Result(statusCode);
			}

			if (!JsonHelpers.TryParse(text, out JObject? json))
			{
				stream.Status = Status.Offline;
				stream.ViewersCount = null;
				
				return new Result(statusCode);
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

			return new Result(statusCode);
		}

		private async Task<Result> UpdateManyAsync(IList<KickStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken)
		{
			IList<Task<Result>> updateTasks = new List<Task<Result>>();

			foreach (KickStream each in streams)
			{
				Task<Result> updateTask = Task.Run(() => UpdateOneAsync(each, preserveSynchronizationContext, cancellationToken));

				updateTasks.Add(updateTask);
			}

			Result[] results = await Task.WhenAll(updateTasks).ConfigureAwait(preserveSynchronizationContext);

			var statuses = results.SelectMany(static result => result.Statuses);

			return new Result(statuses);
		}
	}
}