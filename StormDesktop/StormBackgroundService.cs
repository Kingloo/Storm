using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib;
using StormLib.Interfaces;
using StormLib.Services.Kick;
using static StormLib.Helpers.HttpStatusCodeHelpers;

namespace StormDesktop
{
	public class StormBackgroundService<TStream, TUpdater, TOptionsMonitor, TOptions> : BackgroundService
		where TStream : IStream
		where TUpdater : IUpdater<TStream>
		where TOptionsMonitor : IOptionsMonitor<TOptions>
		where TOptions : IUpdateIntervalOption
	{
		private readonly ILogger<StormBackgroundService<TStream, TUpdater, TOptionsMonitor, TOptions>> logger;
		private readonly TUpdater updater;
		private readonly TOptionsMonitor optionsMonitor;
		private readonly UpdaterMessageQueue updaterMessageQueue;

		private bool isFirstRun = true;

		public StormBackgroundService(
			ILogger<StormBackgroundService<TStream, TUpdater, TOptionsMonitor, TOptions>> logger,
			TUpdater updater,
			TOptionsMonitor optionsMonitor,
			UpdaterMessageQueue updaterMessageQueue)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(updater);
			ArgumentNullException.ThrowIfNull(optionsMonitor);
			ArgumentNullException.ThrowIfNull(updaterMessageQueue);

			this.logger = logger;
			this.updater = updater;
			this.optionsMonitor = optionsMonitor;
			this.updaterMessageQueue = updaterMessageQueue;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogDebug("started background service for {StreamName}", typeof(TStream).Name);

			return base.StartAsync(cancellationToken);
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (isFirstRun)
			{
				await Task.Delay(TimeSpan.FromSeconds(2.5d), stoppingToken).ConfigureAwait(false);

				isFirstRun = false;
			}

			string streamTypeName = typeof(TStream).Name;

			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await RunUpdate(streamTypeName, stoppingToken).ConfigureAwait(false);

					await Task.Delay(optionsMonitor.CurrentValue.UpdateInterval, stoppingToken).ConfigureAwait(false);
				}
			}
			finally
			{
				if (!stoppingToken.IsCancellationRequested)
				{
					logger.LogWarning("background service for {ServiceName} stopped unexpectedly", streamTypeName);
				}
			}
		}

		private async ValueTask RunUpdate(string streamTypeName, CancellationToken cancellationToken)
		{
			IReadOnlyList<TStream> streams = updaterMessageQueue.StreamSource.OfType<TStream>().ToList();

			IList<Result<TStream>> results = new List<Result<TStream>>();

			try
			{
				results = await updater.UpdateAsync(streams, cancellationToken).ConfigureAwait(false);
			}
			catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException innerEx)
			{
				logger.LogError(innerEx, "{Message}", ex.Message);
			}

			foreach (Result<TStream> each in results)
			{
				updaterMessageQueue.ResultsQueue.Enqueue(each);

				if (each.StatusCode != System.Net.HttpStatusCode.OK)
				{
					logger.LogWarning("updating {DisplayName} on {ServiceName} was {StatusCode}", each.Stream.DisplayName, streamTypeName, FormatStatusCode(each.StatusCode));
				}
				else if (typeof(TStream) == typeof(KickStream))
				{
					logger.LogWarning("updating {DisplayName} on {ServiceName} was {StatusCode}", each.Stream.DisplayName, streamTypeName, FormatStatusCode(each.StatusCode));
				}
			}
		}
	}
}