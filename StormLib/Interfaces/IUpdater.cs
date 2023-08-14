using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StormLib.Services;

namespace StormLib.Interfaces
{
	public interface IUpdater<TStream> where TStream : IStream
	{
		public UpdaterType UpdaterType { get; }

		public Task<IReadOnlyList<Result<TStream>>> UpdateAsync(IReadOnlyList<TStream> streams);
		public Task<IReadOnlyList<Result<TStream>>> UpdateAsync(IReadOnlyList<TStream> streams, CancellationToken cancellationToken);
	}
}
