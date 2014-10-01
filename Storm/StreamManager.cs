using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    class StreamManager : ViewModelBase
    {
        #region Fields
        private readonly string urlsFilename = string.Format(@"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
        private DispatcherTimer updateTimer = new DispatcherTimer();
        private readonly MainWindow mainWindow = null;
        #endregion

        #region Properties
        private ObservableCollection<StreamBase> _streams = new ObservableCollection<StreamBase>();
        public ObservableCollection<StreamBase> Streams { get { return this._streams; } }
        #endregion

        #region Commands
        private DelegateCommand<object> _openFeedsFileCommand = null;
        public DelegateCommand<object> OpenFeedsFileCommand
        {
            get
            {
                if (this._openFeedsFileCommand == null)
                {
                    this._openFeedsFileCommand = new DelegateCommand<object>(OpenFeedsFile, canExecute);
                }

                return this._openFeedsFileCommand;
            }
        }

        private DelegateCommandAsync<object> _loadUrlsFromFileCommandAsync = null;
        public DelegateCommandAsync<object> LoadUrlsFromFileCommandAsync
        {
            get
            {
                if (this._loadUrlsFromFileCommandAsync == null)
                {
                    this._loadUrlsFromFileCommandAsync = new DelegateCommandAsync<object>(new Func<object, Task>(LoadUrlsFromFileAsync), canExecute);
                }

                return this._loadUrlsFromFileCommandAsync;
            }
        }

        private DelegateCommandAsync<object> _updateAllCommandAsync = null;
        public DelegateCommandAsync<object> UpdateAllCommandAsync
        {
            get
            {
                if (this._updateAllCommandAsync == null)
                {
                    this._updateAllCommandAsync = new DelegateCommandAsync<object>(new Func<object, Task>(UpdateAllAsync), canExecute);
                }

                return this._updateAllCommandAsync;
            }
        }
        #endregion

        public StreamManager(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            
            this.mainWindow.Loaded += mainWindow_Loaded;
            
            this.updateTimer.Interval = new TimeSpan(0, 6, 0);
            this.updateTimer.Tick += updateTimer_Tick;
            this.updateTimer.IsEnabled = true;
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateAllAsync(null).ConfigureAwait(false);
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUrlsFilename();

            await LoadUrlsFromFileAsync(null);
        }

        private void CheckUrlsFilename()
        {
            if (File.Exists(this.urlsFilename) == false)
            {
                using (FileStream fs = new FileStream(this.urlsFilename, FileMode.Create)) { }
            }
        }

        public async Task LoadUrlsFromFileAsync(object _)
        {
            this.Streams.Clear();

            using (FileStream fsAsync = new FileStream(this.urlsFilename, FileMode.Open, FileAccess.Read, FileShare.None, 1023, true))
            {
                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    string line = string.Empty;

                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        AddService(line);
                    }
                }
            }

            await UpdateAllAsync(null).ConfigureAwait(false);
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

        public async Task UpdateAllAsync(object _)
        {
            VisualStateManager.GoToState(this.mainWindow, "Updating", false);

            await Task.WhenAll(from each in Streams select each.UpdateAsync());

            VisualStateManager.GoToState(this.mainWindow, "Stable", false);
        }

        private void OpenFeedsFile(object _)
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

        private bool canExecute(object parameter)
        {
            // no need for any special logic, no reason to ever deny this
            return true;
        }
    }
}
