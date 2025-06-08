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

		Task LoadStreamsAsync();
		Task LoadStreamsAsync(CancellationToken cancellationToken);

		void StartListeningToMessageQueue();
		void StopListeningToQueue();

		DelegateCommandAsync LoadStreamsCommand { get; }
		DelegateCommand<IStream> OpenPageCommand { get; }
		DelegateCommand<IStream> OpenStreamCommand { get; }
		DelegateCommand<Window> ExitCommand { get; }
	}
}
