using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using StormLib;
using StormLib.Interfaces;
using StormLib.Services;

namespace StormDesktop
{
	public class UpdaterMessageQueue
	{
		private readonly IServiceScopeFactory serviceScopeFactory;

		public ConcurrentQueue<object> ResultsQueue { get; } = new ConcurrentQueue<object>();
		public IReadOnlyCollection<IStream> StreamSource { get; set; } = new Collection<IStream>();

		public UpdaterMessageQueue(IServiceScopeFactory serviceScopeFactory)
		{
			ArgumentNullException.ThrowIfNull(serviceScopeFactory);

			this.serviceScopeFactory = serviceScopeFactory;
		}

		public void SetStreamSource(IReadOnlyCollection<IStream> streamSource)
		{
			ArgumentNullException.ThrowIfNull(streamSource);

			StreamSource = streamSource;
		}

		public async Task UpdateAsync<TStream>(TStream stream, CancellationToken cancellationToken) where TStream : IStream
		{
			ArgumentNullException.ThrowIfNull(stream);

			using IServiceScope serviceScope = serviceScopeFactory.CreateScope();

			IUpdater<TStream> updater = serviceScope.ServiceProvider.GetRequiredService<IUpdater<TStream>>();

			IList<Result<TStream>> result = await updater.UpdateAsync(new[] { stream }, cancellationToken).ConfigureAwait(false);

			ResultsQueue.Enqueue(result[0]);
		}
	}
}