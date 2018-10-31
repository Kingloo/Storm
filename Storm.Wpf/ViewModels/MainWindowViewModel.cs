using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        /// <summary>
        /// Is any asynchronous operation in progress.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, nameof(IsActive));
        }

        private readonly ObservableCollection<IStream> _streams = new ObservableCollection<IStream>();
        /// <summary>
        /// The streams.
        /// </summary>
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

        /// <summary>
        /// Updates the status of every stream in Streams.
        /// </summary>
        /// <returns></returns>
        public Task RefreshAsync() => RefreshAsync(Streams);

        /// <summary>
        /// Updates the status of the supplied streams.
        /// </summary>
        /// <param name="streams">The streams you want to update.</param>
        /// <returns></returns>
        public Task RefreshAsync(IEnumerable<IStream> streams)
        {
            var updateTasks = new List<Task>
            {
                TwitchService.UpdateAsync(_streams.OfType<TwitchStream>()),
                ChaturbateService.UpdateAsync(_streams.OfType<ChaturbateStream>())
            };

            return Task.WhenAll(updateTasks);
        }

        /// <summary>
        /// Loads streams from the on disk file,
        /// adds any new ones to the view model, and removes any that are no longer present.
        /// </summary>
        /// <returns></returns>
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

            await RefreshAsync(loadedStreams);
        }

        /// <summary>
        /// Removes streams from the view model that are no longer in the on-disk file.
        /// </summary>
        /// <param name="loadedStreams">The streams newly loaded from disk.</param>
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

        /// <summary>
        /// Adds streams from the on-disk file that the view model doesn't have yet.
        /// </summary>
        /// <param name="loadedStreams">The streams newly loaded from disk.</param>
        /// <returns></returns>
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
    }
}
