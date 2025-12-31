using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StormLib;
using StormLib.Interfaces;

namespace StormDesktop
{
	public class UpdaterMessageQueue
	{
		private readonly ILogger<UpdaterMessageQueue> logger;

		public ConcurrentQueue<object> ResultsQueue { get; } = new ConcurrentQueue<object>();
		public IReadOnlyCollection<IStream> StreamSource { get; set; } = new Collection<IStream>();

		public UpdaterMessageQueue(ILogger<UpdaterMessageQueue> logger)
		{
			ArgumentNullException.ThrowIfNull(logger);

			this.logger = logger;
		}

		public void SetStreamSource(IReadOnlyCollection<IStream> streamSource)
		{
			ArgumentNullException.ThrowIfNull(streamSource);

			logger.LogDebug(
				"updater message queue stream source set with collection of {Count} {PluralisedWordItem}",
				streamSource.Count,
				streamSource.Count == 1 ? "item" : "items");

			StreamSource = streamSource;
		}
	}
}
