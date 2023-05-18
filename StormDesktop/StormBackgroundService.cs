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
		private string serviceName = typeof(TStream).Name;

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

			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await RunUpdate(stoppingToken).ConfigureAwait(false);

					TimeSpan updateInterval = TimeSpan.FromSeconds(optionsMonitor.CurrentValue.UpdateIntervalSeconds);

					await Task.Delay(updateInterval, stoppingToken).ConfigureAwait(false);
				}
			}
			// catch (Exception ex)
			// {
			// 	logger.LogError(ex, "{ExceptionType}: {ExceptionMessage}", ex.InnerException?.GetType().FullName ?? "no inner ex", ex.Message);	
			// }
			finally
			{
				if (!stoppingToken.IsCancellationRequested)
				{
					logger.LogCritical("background service for {ServiceName} stopped unexpectedly", serviceName);
				}
			}
		}

		private async ValueTask RunUpdate(CancellationToken cancellationToken)
		{
			IReadOnlyList<TStream> streams = updaterMessageQueue.StreamSource.OfType<TStream>().ToList();

			if (!streams.Any())
			{
				return;
			}

			if (String.Equals(serviceName, typeof(TStream).Name, StringComparison.Ordinal))
			{
				serviceName = streams[0].ServiceName;
			}

			IList<Result<TStream>> results = new List<Result<TStream>>();

			try
			{
				int streamCount = streams.Count;
				string streamPluralized = streams.Count == 1 ? "stream" : "streams";
				string streamNames = String.Join(',', streams.Select(s => s.DisplayName));

				logger.LogDebug("updating {Count} {ServiceName} {StreamPluralized} ({StreamNames})", streamCount, serviceName, streamPluralized, streamNames);

				results = await updater.UpdateAsync(streams, cancellationToken).ConfigureAwait(false);
			}
			catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException innerEx)
			{
				logger.LogError(innerEx, "Timeout exception while updating {StreamTypeName}: {Message}", serviceName, innerEx.Message);
			}

			foreach (Result<TStream> each in results)
			{
				updaterMessageQueue.ResultsQueue.Enqueue(each);

				if (each.StatusCode != System.Net.HttpStatusCode.OK)
				{
					logger.LogWarning("updating {DisplayName} ({ServiceName}) was {StatusCode}", each.Stream.DisplayName, serviceName, FormatStatusCode(each.StatusCode));
				}
			}
		}
	}
}
