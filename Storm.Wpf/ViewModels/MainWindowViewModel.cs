using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Storm.Wpf.Common;
using Storm.Wpf.Streams;
using Storm.Wpf.StreamServices;

namespace Storm.Wpf.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields
        private readonly FileLoader fileLoader = null;
        #endregion

        #region Properties
        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, nameof(IsActive));
        }

        private readonly ObservableCollection<IStream> _streams = new ObservableCollection<IStream>();
        public IReadOnlyCollection<IStream> Streams => _streams;
        #endregion

        #region Commands
        private DelegateCommand<Window> _exitCommand = null;
        public DelegateCommand<Window> ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand<Window>(window => window.Close(), _ => true);
                }

                return _exitCommand;
            }
        }

        private DelegateCommandAsync _loadStreamsCommand = null;
        public DelegateCommandAsync LoadStreamsCommand
        {
            get
            {
                if (_loadStreamsCommand == null)
                {
                    _loadStreamsCommand = new DelegateCommandAsync(LoadStreams, canExecuteAsync);
                }

                return _loadStreamsCommand;
            }
        }

        private bool canExecuteAsync(object _) => !IsActive;

        private bool canExecute(object _) => true;
        #endregion

        public MainWindowViewModel(FileLoader fileLoader)
        {
            this.fileLoader = fileLoader ?? throw new ArgumentNullException(nameof(fileLoader));
        }

        public Task RefreshAsync() => ServicesManager.UpdateAsync(Streams);

        public async Task LoadStreams()
        {
            string[] lines = await fileLoader.LoadLinesAsync();

            List<IStream> loadedStreams = new List<IStream>();

            foreach (string line in lines)
            {
                if (StreamFactory.TryCreate(line, out IStream stream))
                {
                    loadedStreams.Add(stream);
                }
            }

            RemoveOld(loadedStreams);

            var newlyAdded = AddNew(loadedStreams);

            await ServicesManager.UpdateAsync(newlyAdded);
        }

        private IEnumerable<IStream> AddNew(IEnumerable<IStream> loadedStreams)
        {
            List<IStream> added = new List<IStream>();

            foreach (IStream each in loadedStreams)
            {
                if (!_streams.Contains(each))
                {
                    _streams.Add(each);

                    added.Add(each);
                }
            }

            return added;
        }

        private void RemoveOld(IEnumerable<IStream> loadedStreams)
        {
            List<IStream> toBeRemoved = new List<IStream>();

            foreach (IStream each in _streams)
            {
                if (!loadedStreams.Contains(each))
                {
                    toBeRemoved.Add(each);
                }
            }

            foreach (IStream each in toBeRemoved)
            {
                _streams.Remove(each);
            }
        }
    }
}
