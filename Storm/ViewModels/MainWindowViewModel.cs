using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Storm.DataAccess;
using Storm.Extensions;
using Storm.Model;

namespace Storm.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields
        private readonly MainWindow mainWindow = null;
        private readonly IRepository urlsRepo = null;

        private readonly DispatcherTimer updateTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 3, 0)
        };
        #endregion

        #region Properties
        private bool _activity = false;
        public bool Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                if (_activity != value)
                {
                    _activity = value;

                    RaisePropertyChanged(nameof(Activity));

                    RaiseAllAsyncCanExecuteChangedEvents();
                }
            }
        }

        private void RaiseAllAsyncCanExecuteChangedEvents()
        {
            ReloadUrlsAsyncCommand.RaiseCanExecuteChanged();
            UpdateAllAsyncCommand.RaiseCanExecuteChanged();
        }

        private readonly ObservableCollection<StreamBase> _streams = new ObservableCollection<StreamBase>();
        //public ObservableCollection<StreamBase> Streams { get { return _streams; } }
        public IReadOnlyCollection<StreamBase> Streams { get { return _streams; } }
        #endregion

        #region Commands
        private DelegateCommand<StreamBase> _goToStreamCommand = null;
        public DelegateCommand<StreamBase> GoToStreamCommand
        {
            get
            {
                if (_goToStreamCommand == null)
                {
                    _goToStreamCommand = new DelegateCommand<StreamBase>(GoToStream, canExecute);
                }

                return _goToStreamCommand;
            }
        }

        private static void GoToStream(StreamBase stream)
        {
            stream.GoToStream();
        }

        private DelegateCommand _openUrlsFileCommand = null;
        public DelegateCommand OpenUrlsFileCommand
        {
            get
            {
                if (_openUrlsFileCommand == null)
                {
                    _openUrlsFileCommand = new DelegateCommand(OpenUrlsFile, canExecute);
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
                Process.Start("notepad.exe", urlsRepo.FilePath);
            }
            catch (FileNotFoundException)
            {
                Process.Start(urlsRepo.FilePath); // .txt default program
            }
        }

        private DelegateCommandAsync _reloadUrlsAsyncCommand = null;
        public DelegateCommandAsync ReloadUrlsAsyncCommand
        {
            get
            {
                if (_reloadUrlsAsyncCommand == null)
                {
                    _reloadUrlsAsyncCommand = new DelegateCommandAsync(ReloadUrlsAsync, canExecuteAsync);
                }

                return _reloadUrlsAsyncCommand;
            }
        }

        public async Task LoadUrlsAsync()
        {
            _streams.Clear();

            IEnumerable<StreamBase> loaded = await urlsRepo.LoadAsync();
            
            _streams.AddList(loaded);
        }

        private async Task ReloadUrlsAsync()
        {
            await LoadUrlsAsync();
            
            await UpdateAsync();
        }

        private DelegateCommandAsync _updateAllAsyncCommand = null;
        public DelegateCommandAsync UpdateAllAsyncCommand
        {
            get
            {
                if (_updateAllAsyncCommand == null)
                {
                    _updateAllAsyncCommand = new DelegateCommandAsync(UpdateAsync, canExecuteAsync);
                }

                return _updateAllAsyncCommand;
            }
        }

        public async Task UpdateAsync()
        {
            SetUIToUpdating();

            List<Task> updateTasks = (from each in Streams
                                      where !each.Updating
                                      select each.UpdateAsync())
                                      .ToList();

            if (updateTasks.Count > 0)
            {
                await Task.WhenAll(updateTasks);
            }
            
            SetUIToStable();
        }

        private void SetUIToUpdating()
        {
            Activity = true;

            VisualStateManager.GoToState(mainWindow, "Updating", false);
        }

        private void SetUIToStable()
        {
            VisualStateManager.GoToState(mainWindow, "Stable", false);

            Activity = false;
        }

        private DelegateCommand _exitCommand = null;
        public DelegateCommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand(Exit, canExecute);
                }

                return _exitCommand;
            }
        }

        private void Exit()
        {
            mainWindow.Close();
        }

        private bool canExecute(object _)
        {
            return true;
        }

        private bool canExecuteAsync(object _)
        {
            return !Activity;
        }
        #endregion

        public MainWindowViewModel(MainWindow window, IRepository urlsRepo)
        {
            this.urlsRepo = urlsRepo;

            mainWindow = window;

            updateTimer.Tick += async (s, e) => await UpdateAsync();
            updateTimer.Start();
        }
    }
}
