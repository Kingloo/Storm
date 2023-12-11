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
using static StormLib.Common.ExceptionFilterUtility;
using static StormLib.Helpers.HttpStatusCodeHelpers;

namespace StormDesktop
{
	public class StormBackgroundService<TStream, TUpdater, TOptionsMonitor, TOptions> : BackgroundService
		where TStream : IStream
		where TUpdater : IUpdater<TStream>
		where TOptionsMonitor : IOptionsMonitor<TOptions>
		where TOptions : IUpdateIntervalOption
	{
		private readonly ILogger logger;
		private readonly TUpdater updater;
		private readonly TOptionsMonitor optionsMonitor;
		private readonly UpdaterMessageQueue updaterMessageQueue;

		public StormBackgroundService(
			ILoggerFactory loggerFactory,
			TUpdater updater,
			TOptionsMonitor optionsMonitor,
			UpdaterMessageQueue updaterMessageQueue)
		{
			ArgumentNullException.ThrowIfNull(loggerFactory);
			ArgumentNullException.ThrowIfNull(updater);
			ArgumentNullException.ThrowIfNull(optionsMonitor);
			ArgumentNullException.ThrowIfNull(updaterMessageQueue);

			this.logger = loggerFactory.CreateLogger($"StormDesktop.Services.{typeof(TStream).Name}");
			this.updater = updater;
			this.optionsMonitor = optionsMonitor;
			this.updaterMessageQueue = updaterMessageQueue;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogDebug("starting");

			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogDebug("stopping");

			return base.StopAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("started");

			await Task.Delay(TimeSpan.FromSeconds(2.5d), stoppingToken).ConfigureAwait(false);

			try
			{
				while (true)
				{
					stoppingToken.ThrowIfCancellationRequested();

					await RunUpdate(stoppingToken).ConfigureAwait(false);

					await Task.Delay(optionsMonitor.CurrentValue.UpdateInterval, stoppingToken).ConfigureAwait(false);
				}
			}
			finally
			{
				if (stoppingToken.IsCancellationRequested)
				{
					logger.LogDebug("stopped (cancelled)");
				}
				else
				{
					logger.LogCritical("stopped unexpectedly");
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

			IReadOnlyList<Result<TStream>> results = new List<Result<TStream>>();

			try
			{
				int streamCount = streams.Count;
				string accountPluralized = streams.Count == 1 ? "account" : "accounts";
				string streamNames = String.Join(',', streams.Select(static s => s.DisplayName));

				results = await updater.UpdateAsync(streams, cancellationToken).ConfigureAwait(false);

				logger.LogDebug("updated {Count} {StreamPluralized} ({StreamNames})", streamCount, accountPluralized, streamNames);
			}
			catch (TaskCanceledException ex) when (ex.InnerException is not null)
			{
				logger.LogDebug(ex, "update cancelled ('{InnerExceptionType}': '{InnerExceptionMessage}')", ex.InnerException.GetType().FullName, ex.InnerException.Message);
			}
			catch (Exception ex) when (False(() => logger.LogError(ex, "run update threw: '{ExceptionType}', '{ExceptionMessage}'", ex.GetType().Name, ex.Message)))
			{
				throw;
			}

			foreach (Result<TStream> each in results)
			{
				updaterMessageQueue.ResultsQueue.Enqueue(each);

				if (each.StatusCode != System.Net.HttpStatusCode.OK)
				{
					if (streams[0] is KickStream) // Kick is access-restricted behind CloudFlare and takes some time to get passed that
					{
						continue;
					}
					else
					{
						logger.LogWarning("updating {DisplayName} was {StatusCode}", each.Stream.DisplayName, FormatStatusCode(each.StatusCode));
					}
				}
			}
		}
	}
}
