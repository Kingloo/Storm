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
			logger.LogDebug("start");

			return base.StartAsync(cancellationToken);
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.LogDebug("execute for {ServiceName}", nameof(TStream));

			try
			{
				string serviceName = typeof(TStream).Name;

				while (!stoppingToken.IsCancellationRequested)
				{
					logger.LogDebug("start updating {ServiceName} streams", serviceName);

					await RunUpdate(serviceName, stoppingToken).ConfigureAwait(false);

					logger.LogInformation("updated {ServiceName}", serviceName);

					await Task.Delay(optionsMonitor.CurrentValue.UpdateInterval, stoppingToken).ConfigureAwait(false);
				}
			}
			finally
			{
				if (!stoppingToken.IsCancellationRequested)
				{
					logger.LogWarning("background service for {ServiceName} stopped", nameof(TStream));
				}
			}
		}

		private async ValueTask RunUpdate(string serviceName, CancellationToken cancellationToken)
		{
			IReadOnlyList<TStream> streams = updaterMessageQueue.StreamSource.OfType<TStream>().ToList();

			IList<Result<TStream>> results = await updater.UpdateAsync(streams, cancellationToken).ConfigureAwait(false);

			logger.LogDebug("update completed for {ServiceName} with {Count} results", serviceName, results.Count);

			foreach (Result<TStream> each in results)
			{
				updaterMessageQueue.ResultsQueue.Enqueue(each);
			}
		}
	}
}