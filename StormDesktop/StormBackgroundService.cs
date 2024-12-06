using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormLib;
using StormLib.Interfaces;
using StormLib.Services;
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
			logger.LogDebug("started");

			await Task.Delay(TimeSpan.FromSeconds(1.5d), stoppingToken).ConfigureAwait(false);

			ExceptionDispatchInfo? edi = null;

			while (true)
			{
				stoppingToken.ThrowIfCancellationRequested();

				try
				{
					await RunUpdate(stoppingToken).ConfigureAwait(false);

					await Task.Delay(optionsMonitor.CurrentValue.UpdateInterval, stoppingToken).ConfigureAwait(false);
				}
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception ex)
				{
					edi = ExceptionDispatchInfo.Capture(ex);
				}
#pragma warning restore CA1031
				finally
				{
					if (stoppingToken.IsCancellationRequested)
					{
						logger.LogDebug("stopped (cancelled)");
					}
					else
					{
						if (edi is not null)
						{
							logger.LogCritical(
								edi.SourceException,
								"stopped unexpectedly: ({Type}: {Message})",
								edi.SourceException.GetType(),
								edi.SourceException.Message);
						}
					}
				}
			}
		}

		private async ValueTask RunUpdate(CancellationToken cancellationToken)
		{
			List<TStream> streams = updaterMessageQueue.StreamSource.OfType<TStream>().ToList();

			if (streams.Count == 0)
			{
				return;
			}

			IReadOnlyList<Result<TStream>> results = new List<Result<TStream>>(capacity: streams.Count);

			try
			{
				int streamCount = streams.Count;
				string accountPluralized = streams.Count == 1 ? "account" : "accounts";
				string streamNames = String.Join(',', streams.Select(static s => s.DisplayName));

				results = await updater.UpdateAsync(streams, cancellationToken).ConfigureAwait(false);

				logger.LogDebug("updated {Count} {StreamPluralized} ({StreamNames})", streamCount, accountPluralized, streamNames);
			}
			catch (OperationCanceledException ex)
			{
				logger.LogDebug(ex, "update cancelled");
			}
			catch (Exception ex) when (False(() => logger.LogError(ex, "run update threw: '{ExceptionType}', '{ExceptionMessage}'", ex.GetType().Name, ex.Message)))
			{
				throw;
			}

			switch (updater.UpdaterType)
			{
				case UpdaterType.One:
					HandleUpdaterTypeOne(results);
					break;
				case UpdaterType.Many:
					HandleUpdaterTypeMany(results);
					break;
				case UpdaterType.None:
				case UpdaterType.Unknown:
				default:
					logger.LogCritical("updater type was {UpdaterType}", updater.UpdaterType);
					break;
			}
		}

		private void HandleUpdaterTypeOne(IReadOnlyList<Result<TStream>> results)
		{
			if (results.Count == 0)
			{
				return;
			}

			foreach (Result<TStream> each in results)
			{
				updaterMessageQueue.ResultsQueue.Enqueue(each);

				if (each.StatusCode != System.Net.HttpStatusCode.OK)
				{
					if (results[0].Stream is KickStream)
					{
						// Kick is access-restricted behind CloudFlare and takes some time to get passed that

						continue;
					}
					else
					{
						logger.LogWarning("updating {DisplayName} was {StatusCode}", each.Stream.DisplayName, FormatStatusCode(each.StatusCode));
					}
				}
			}
		}

		private void HandleUpdaterTypeMany(IReadOnlyList<Result<TStream>> results)
		{
			foreach (Result<TStream> each in results)
			{
				updaterMessageQueue.ResultsQueue.Enqueue(each);
			}

			Result<TStream> first = results[0];

			if (first.StatusCode != System.Net.HttpStatusCode.OK)
			{
				logger.LogWarning("update was {StatusCode}", FormatStatusCode(first.StatusCode));
			}
		}
	}
}
