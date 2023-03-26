using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using StormDesktop.Common;
using StormLib.Interfaces;

namespace StormDesktop.Interfaces
{
	public interface IMainWindowViewModel
	{
		IReadOnlyCollection<IStream> Streams { get; }

		Task UpdateAsync();
		Task UpdateAsync(CancellationToken cancellationToken);
		Task UpdateAsync(IEnumerable<IStream> streams);
		Task UpdateAsync(IEnumerable<IStream> streams, CancellationToken cancellationToken);

		Task LoadStreamsAsync();
		Task LoadStreamsAsync(CancellationToken cancellationToken);

		DelegateCommandAsync UpdateCommand { get; }
		DelegateCommandAsync LoadStreamsCommand { get; }
		DelegateCommand<IStream> OpenPageCommand { get; }
		DelegateCommand<IStream> OpenStreamCommand { get; }
		DelegateCommand<Window> ExitCommand { get; }
	}
}
