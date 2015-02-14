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
    public class StreamManager : ViewModelBase
    {
        #region Fields
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
        private DelegateCommand<StreamBase> _goToStreamCommand = null;
        public DelegateCommand<StreamBase> GoToStreamCommand
        {
            get
            {
                if (this._goToStreamCommand == null)
                {
                    this._goToStreamCommand = new DelegateCommand<StreamBase>(GoToStream, canExecute);
                }

                return this._goToStreamCommand;
            }
        }

        private void GoToStream(StreamBase stream)
        {
            Utils.OpenUriInBrowser(stream.Uri);
        }

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
                Process.Start("notepad.exe", Program.StormUrlsFilePath);
            }
            catch (FileNotFoundException)
            {
                Process.Start("wordpad.exe", Program.StormUrlsFilePath);
            }
        }

        private DelegateCommandAsync _loadUrlsFromFileCommandAsync = null;
        public DelegateCommandAsync LoadUrlsFromFileCommandAsync
        {
            get
            {
                if (this._loadUrlsFromFileCommandAsync == null)
                {
                    this._loadUrlsFromFileCommandAsync = new DelegateCommandAsync(LoadUrlsFromFileAsync, canExecuteAsync);
                }

                return this._loadUrlsFromFileCommandAsync;
            }
        }

        public async Task LoadUrlsFromFileAsync()
        {
            this.Activity = true;

            this.Streams.Clear();

            IEnumerable<string> loaded = await Program.LoadUrlsFromFile();

            CreateStreamBasesFromStrings(loaded);

            await UpdateAllAsync();

            this.Activity = false;
        }

        private void CreateStreamBasesFromStrings(IEnumerable<string> loaded)
        {
            foreach (string each in loaded)
            {
                AddService(each);
            }
        }

        private DelegateCommandAsync _updateAllCommandAsync = null;
        public DelegateCommandAsync UpdateAllCommandAsync
        {
            get
            {
                if (this._updateAllCommandAsync == null)
                {
                    this._updateAllCommandAsync = new DelegateCommandAsync(UpdateAllAsync, canExecuteAsync);
                }

                return this._updateAllCommandAsync;
            }
        }

        public async Task UpdateAllAsync()
        {
            this.Activity = true;
            
            MainWindow appMainWindow = (MainWindow)Application.Current.MainWindow;

            //if (appMainWindow.CheckAccess())
            //{
                VisualStateManager.GoToState(appMainWindow, "Updating", true);
            //}

            await Task.WhenAll(from each in Streams
                               where (each != null) && (each.Updating == false)
                               select each.UpdateAsync());

            //if (appMainWindow.CheckAccess())
            //{
                VisualStateManager.GoToState(appMainWindow, "Stable", true);
            //}

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

        private bool canExecute(object _)
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
            CreateStreamBasesFromStrings(Program.URLs);

            CreateAndStartTimer();
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAllAsync();
        }

        private void CreateAndStartTimer()
        {
            this.updateTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 4, 0),
                IsEnabled = false
            };

            this.updateTimer.Tick += updateTimer_Tick;
            this.updateTimer.IsEnabled = true;
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
                    sb = new Ustream(each);
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
                case "www.ustream.tv":
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
            sb.AppendLine(string.Format("URLs file: {0}", Program.StormUrlsFilePath));

            foreach (StreamBase each in Streams)
            {
                sb.AppendLine(each.ToString());
            }

            return sb.ToString();
        }
    }
}
