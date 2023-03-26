using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Services;

namespace StormLib.Interfaces
{
	public interface IUpdater<TStream> where TStream : IStream
	{
		public UpdaterType UpdaterType { get; }

		public Task<Result[]> UpdateAsync(IList<TStream> streams);
		public Task<Result[]> UpdateAsync(IList<TStream> streams, bool preserveSynchronizationContext);
		public Task<Result[]> UpdateAsync(IList<TStream> streams, CancellationToken cancellationToken);
		public Task<Result[]> UpdateAsync(IList<TStream> streams, bool preserveSynchronizationContext, CancellationToken cancellationToken);
	}
}