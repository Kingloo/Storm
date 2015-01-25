using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    class StreamManager : ViewModelBase
    {
        #region Fields
        private readonly string urlsFilename = string.Format(@"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
        private DispatcherTimer updateTimer = null;
        #endregion

        #region Properties
        private bool _activity = false;
        public bool Activity
        {
            get
            {
                return this._activity;
            }
            set
            {
                this._activity = value;

                OnNotifyPropertyChanged();

                this.RaiseAllAsyncCanExecuteChangedEvents();
            }
        }

        private void RaiseAllAsyncCanExecuteChangedEvents()
        {
            this.LoadUrlsFromFileCommandAsync.RaiseCanExecuteChanged();
            this.UpdateAllCommandAsync.RaiseCanExecuteChanged();
        }

        private ObservableCollection<StreamBase> _streams = new ObservableCollection<StreamBase>();
        public ObservableCollection<StreamBase> Streams { get { return this._streams; } }
        #endregion

        #region Commands
        private DelegateCommand _openFeedsFileCommand = null;
        public DelegateCommand OpenFeedsFileCommand
        {
            get
            {
                if (this._openFeedsFileCommand == null)
                {
                    this._openFeedsFileCommand = new DelegateCommand(OpenFeedsFile, canExecute);
                }

                return this._openFeedsFileCommand;
            }
        }

        private void OpenFeedsFile()
        {
            try
            {
                Process.Start("notepad.exe", this.urlsFilename);
            }
            catch (FileNotFoundException)
            {
                Process.Start("wordpad.exe", this.urlsFilename);
            }
        }

        private DelegateCommandAsync _loadUrlsFromFileCommandAsync = null;
        public DelegateCommandAsync LoadUrlsFromFileCommandAsync
        {
            get
            {
                if (this._loadUrlsFromFileCommandAsync == null)
                {
                    this._loadUrlsFromFileCommandAsync = new DelegateCommandAsync(new Func<Task>(LoadUrlsFromFileAsync), canExecuteAsync);
                }

                return this._loadUrlsFromFileCommandAsync;
            }
        }

        public async Task LoadUrlsFromFileAsync()
        {
            this.Activity = true;

            this.Streams.Clear();

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(this.urlsFilename, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true);
            }
            catch (FileNotFoundException)
            {
                if (fsAsync != null)
                {
                    fsAsync.Close();
                }

                File.CreateText(this.urlsFilename);

                OpenFeedsFile();

                return;
            }

            using (StreamReader sr = new StreamReader(fsAsync))
            {
                string line = string.Empty;

                while ((line = await sr.ReadLineAsync()) != null)
                {
                    AddService(line);
                }
            }

            if (fsAsync != null)
            {
                fsAsync.Close();
            }

            this.Activity = false;
        }

        private DelegateCommandAsync _updateAllCommandAsync = null;
        public DelegateCommandAsync UpdateAllCommandAsync
        {
            get
            {
                if (this._updateAllCommandAsync == null)
                {
                    this._updateAllCommandAsync = new DelegateCommandAsync(new Func<Task>(UpdateAllAsync), canExecute);
                }

                return this._updateAllCommandAsync;
            }
        }

        public async Task UpdateAllAsync()
        {
            this.Activity = true;

            MainWindow appMainWindow = (MainWindow)Application.Current.MainWindow;

            VisualStateManager.GoToState(appMainWindow, "Updating", false);
            
            IEnumerable<Task> allUpdateTasks = from each in Streams
                                               select each.UpdateAsync();

            await Task.WhenAll(allUpdateTasks);

            VisualStateManager.GoToState(appMainWindow, "Stable", false);

            this.Activity = false;
        }

        private DelegateCommand _exitCommand = null;
        public DelegateCommand ExitCommand
        {
            get
            {
                if (this._exitCommand == null)
                {
                    this._exitCommand = new DelegateCommand(Exit, canExecute);
                }

                return this._exitCommand;
            }
        }

        private void Exit()
        {
            Application.Current.MainWindow.Close();
        }

        private bool canExecute(object parameter)
        {
            // no need for any special logic, no reason to ever deny this
            return true;
        }

        private bool canExecuteAsync(object parameter)
        {
            return !this.Activity;
        }
        #endregion

        public StreamManager()
        {
            this.updateTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 4, 0)
            };

            this.updateTimer.Tick += updateTimer_Tick;
            this.updateTimer.IsEnabled = true;

            Init();
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAllAsync().ConfigureAwait(false);
        }

        public async Task Init()
        {
            await LoadUrlsFromFileAsync();

            await UpdateAllAsync().ConfigureAwait(false);
        }

        private void AddService(string each)
        {
            StreamingService service = DetermineStreamingService(each);
            StreamBase sb = null;

            switch (service)
            {
                case StreamingService.Twitch:
                    sb = new TwitchStream(each);
                    break;
                case StreamingService.Ustream:
                    break;
                case StreamingService.UnsupportedService:
                    break;
                case StreamingService.None:
                    break;
                default:
                    break;
            }

            if (sb != null)
            {
                this.Streams.Add(sb);
            }
        }

        private StreamingService DetermineStreamingService(string s)
        {
            StreamingService ss = StreamingService.None;

            Uri uri = new Uri(s);

            switch (uri.DnsSafeHost)
            {
                case "twitch.tv":
                    ss = StreamingService.Twitch;
                    break;
                case "www.twitch.tv":
                    ss = StreamingService.Twitch;
                    break;
                case "ustream.tv":
                    ss = StreamingService.Ustream;
                    break;
                default:
                    ss = StreamingService.UnsupportedService;
                    break;
            }

            return ss;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.GetType().ToString());
            sb.AppendLine(string.Format("URLs file: {0}", this.urlsFilename));

            if (this.Streams.Count > 0)
            {
                foreach (StreamBase each in Streams)
                {
                    sb.AppendLine(each.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
