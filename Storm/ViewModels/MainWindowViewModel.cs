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

namespace Storm.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Fields
        MainWindow mainWindow = null;
        IEnumerable<StreamBase> activeTasks = null;
        private IRepository urlsRepo = null;
        private readonly DispatcherTimer updateTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 3, 15)
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
                _activity = value;

                OnNotifyPropertyChanged();

                RaiseAllAsyncCanExecuteChangedEvents();
            }
        }

        private void RaiseAllAsyncCanExecuteChangedEvents()
        {
            ReloadUrlsAsyncCommand.RaiseCanExecuteChanged();
            UpdateAllAsyncCommand.RaiseCanExecuteChanged();
        }

        private ObservableCollection<StreamBase> _streams = new ObservableCollection<StreamBase>();
        public ObservableCollection<StreamBase> Streams { get { return _streams; } }
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

        public static void GoToStream(StreamBase stream)
        {
            string args = string.Format(@"/C livestreamer.exe {0} best", stream.Uri.AbsoluteUri);

            ProcessStartInfo pInfo = new ProcessStartInfo
            {
                Arguments = args,
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(pInfo);
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
            // notepad would be the one to notify that urlsRepo.FilePath could not be found/opened

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

        private async Task LoadUrlsAsync()
        {
            Streams.Clear();

            IEnumerable<StreamBase> loaded = await urlsRepo.LoadAsync();

            Streams.AddList<StreamBase>(loaded);
        }

        private async Task ReloadUrlsAsync()
        {
            await LoadUrlsAsync();

            await UpdateAllAsync().ConfigureAwait(false);
        }

        private DelegateCommandAsync _updateAllAsyncCommand = null;
        public DelegateCommandAsync UpdateAllAsyncCommand
        {
            get
            {
                if (_updateAllAsyncCommand == null)
                {
                    _updateAllAsyncCommand = new DelegateCommandAsync(UpdateAllAsync, canExecuteAsync);
                }

                return _updateAllAsyncCommand;
            }
        }

        private async Task UpdateAllAsync()
        {
            SetUIToUpdating();

            IEnumerable<Task> updateTasks = from each in Streams
                                            where each.Updating == false
                                            where each.IsValid
                                            select each.UpdateAsync();

            await Task.WhenAll(updateTasks);

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

        public MainWindowViewModel(MainWindow mainWindow, IRepository urlsRepo)
        {
            this.urlsRepo = urlsRepo;

            this.mainWindow = mainWindow;
            mainWindow.Loaded += mainWindow_Loaded;

            activeTasks = from each in Streams
                          where each.Updating
                          select each;

            updateTimer.Tick += updateTimer_Tick;
            updateTimer.Start();
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUrlsAsync();

            await UpdateAllAsync();
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAllAsync().ConfigureAwait(false);
        }
    }
}
