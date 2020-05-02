using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using StormDesktop.Common;
using StormLib.Interfaces;

namespace StormDesktop.Gui
{
    public interface IMainWindowViewModel
    {
        IReadOnlyCollection<IStream> Streams { get; }

        void StartUpdateTimer(TimeSpan updateFrequency);
        void StopUpdateTimer();

        Task UpdateAsync();
        Task UpdateAsync(IEnumerable<IStream> streams);

        Task LoadStreamsAsync();

        DelegateCommandAsync UpdateCommand { get; }
        DelegateCommandAsync LoadStreamsCommand { get; }
        DelegateCommand<IStream> OpenPageCommand { get; }
        DelegateCommand<IStream> OpenStreamCommand { get; }
        DelegateCommand<Window> ExitCommand { get; }

        void CleanUp();
    }
}
