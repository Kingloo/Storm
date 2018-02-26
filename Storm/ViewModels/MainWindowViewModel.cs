using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Storm.Common;
using Storm.DataAccess;
using Storm.Model;

namespace Storm.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Events
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        private void OnStatusChanged(bool isUpdating)
            => StatusChanged?.Invoke(this, new StatusChangedEventArgs(isUpdating));
        #endregion

        #region Fields
        private readonly TxtRepo urlsRepo = null;

        private readonly DispatcherTimer updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(3d)
        };
        #endregion

        #region Properties
        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;

                    RaisePropertyChanged(nameof(IsActive));

                    RaiseAllAsyncCanExecuteChangedEvents();

                    OnStatusChanged(_isActive);
                }
            }
        }

        private void RaiseAllAsyncCanExecuteChangedEvents()
        {
            LoadUrlsAsyncCommand.RaiseCanExecuteChanged();
            UpdateAllAsyncCommand.RaiseCanExecuteChanged();
        }

        private readonly ObservableCollection<StreamBase> _streams
            = new ObservableCollection<StreamBase>();
        public IReadOnlyCollection<StreamBase> Streams => _streams;
        #endregion

        #region Commands
        private DelegateCommand<StreamBase> _goToStreamCommand = null;
        public DelegateCommand<StreamBase> GoToStreamCommand
        {
            get
            {
                if (_goToStreamCommand == null)
                {
                    _goToStreamCommand = new DelegateCommand<StreamBase>(GoToStream, CanExecute);
                }

                return _goToStreamCommand;
            }
        }

        public void GoToStream(StreamBase stream)
        {
            if (stream == null) { throw new ArgumentNullException(nameof(stream)); }

            stream.GoToStream();
        }

        private DelegateCommand _openUrlsFileCommand = null;
        public DelegateCommand OpenUrlsFileCommand
        {
            get
            {
                if (_openUrlsFileCommand == null)
                {
                    _openUrlsFileCommand = new DelegateCommand(OpenUrlsFile, CanExecute);
                }

                return _openUrlsFileCommand;
            }
        }

        private void OpenUrlsFile() => urlsRepo.OpenFile();

        private DelegateCommandAsync _loadUrlsAsyncCommand = null;
        public DelegateCommandAsync LoadUrlsAsyncCommand
        {
            get
            {
                if (_loadUrlsAsyncCommand == null)
                {
                    _loadUrlsAsyncCommand = new DelegateCommandAsync(LoadUrlsAsync, CanExecuteAsync);
                }

                return _loadUrlsAsyncCommand;
            }
        }

        public async Task LoadUrlsAsync()
        {
            _streams.Clear();

            string[] loaded = await urlsRepo.LoadAsync();

            foreach (string each in loaded)
            {
                if (StreamFactory.TryCreate(each, out StreamBase stream))
                {
                    _streams.Add(stream);
                }
            }
            
            await UpdateAsync();
        }
        
        private DelegateCommandAsync _updateAllAsyncCommand = null;
        public DelegateCommandAsync UpdateAllAsyncCommand
        {
            get
            {
                if (_updateAllAsyncCommand == null)
                {
                    _updateAllAsyncCommand = new DelegateCommandAsync(UpdateAsync, CanExecuteAsync);
                }

                return _updateAllAsyncCommand;
            }
        }

        public async Task UpdateAsync()
        {
            IsActive = true;

            var updateTasks = Streams
                .Where(x => !x.Updating)
                .Select(x => x.UpdateAsync())
                .ToList();

            if (updateTasks.Count > 0)
            {
                await Task.WhenAll(updateTasks);
            }

            IsActive = false;
        }
        
        private bool CanExecute(object _) => true;

        private bool CanExecuteAsync(object _) => !IsActive;
        #endregion

        public MainWindowViewModel(TxtRepo urlsRepo)
        {
            this.urlsRepo = urlsRepo ?? throw new ArgumentNullException(nameof(urlsRepo));

            updateTimer.Tick += async (s, e) => await UpdateAsync();
            updateTimer.Start();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().ToString());
            sb.AppendLine(IsActive ? "IsActive: true" : "IsActive: false");

            sb.Append("no. of streams: ");
            sb.AppendLine(Streams.Count.ToString());

            sb.AppendLine(urlsRepo?.ToString() ?? "urlsRepo is null");
            sb.AppendLine(updateTimer?.ToString() ?? "updateTimer is null");

            return sb.ToString();
        }
    }
}
