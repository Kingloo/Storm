using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Storm.Common;
using Storm.DataAccess;
using Storm.Extensions;
using Storm.Model;

namespace Storm.ViewModels
{
    public class StatusChangedEventArgs : EventArgs
    {
        private readonly bool _isUpdating = false;
        public bool IsUpdating => _isUpdating;

        public StatusChangedEventArgs(bool isUpdating)
        {
            _isUpdating = isUpdating;
        }
    }

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
            Interval = TimeSpan.FromMinutes(3)
        };
        #endregion

        #region Properties
        private bool _activity = false;
        public bool Activity
        {
            get => _activity;
            set
            {
                if (_activity != value)
                {
                    _activity = value;

                    RaisePropertyChanged(nameof(Activity));

                    RaiseAllAsyncCanExecuteChangedEvents();

                    OnStatusChanged(_activity);
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

        private static void GoToStream(StreamBase stream)
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

        private void OpenUrlsFile()
        {
            // The FileNotFoundException will be for notepad.exe, NOT urlsRepo.FilePath
            // the file path is an argument
            // notepad will tell you if urlsRepo.FilePath could not be found/opened

            try
            {
                Process.Start("notepad.exe", urlsRepo.UrlsFile.FullName);
            }
            catch (FileNotFoundException ex)
            {
                Log.LogException(ex);

                Process.Start(urlsRepo.UrlsFile.FullName); // .txt default program
            }
        }

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

            var loaded = await urlsRepo.LoadAsync();
            
            _streams.AddRange(loaded);

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
            Activity = true;

            var updateTasks = Streams
                .Where(x => !x.Updating)
                .Select(x => x.UpdateAsync())
                .ToList();

            if (updateTasks.Count > 0)
            {
                await Task.WhenAll(updateTasks);
            }

            Activity = false;
        }
        
        private bool CanExecute(object _) => true;

        private bool CanExecuteAsync(object _) => !Activity;
        #endregion

        public MainWindowViewModel(TxtRepo urlsRepo)
        {
            this.urlsRepo = urlsRepo;

            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
            => await UpdateAsync();
    }
}
