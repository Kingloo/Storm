using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Storm
{
    class StreamManager : ViewModelBase
    {
        #region Hidden
        private readonly string urlsFilename = string.Format(@"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
        private DispatcherTimer updateTimer = new DispatcherTimer();
        #endregion

        #region Visible
        public ObservableCollection<StreamBase> Streams { get; private set; }
        #endregion

        #region Commands
        private DelegateCommand _goToStreamCommand = null;
        public DelegateCommand GoToStreamCommand { get { return this._goToStreamCommand; } }
        private DelegateCommand _openFeedsFileCommand = null;
        public DelegateCommand OpenFeedsFileCommand { get { return this._openFeedsFileCommand; } }
        #endregion

        public StreamManager()
        {
            InitializePublicMembers();

            CheckUrlsFilename();
        }

        private void InitializePublicMembers()
        {
            this._openFeedsFileCommand = new DelegateCommand(OpenFeedsFile, canExecuteCommand);
            this._goToStreamCommand = new DelegateCommand(GoToStream, canExecuteCommand);

            this.Streams = new ObservableCollection<StreamBase>();

            this.updateTimer.Tick += async (sender, e) =>
                {
                    await this.UpdateAllAsync();
                };
            this.updateTimer.Interval = new TimeSpan(0, 2, 30);
            this.updateTimer.IsEnabled = true;
        }

        private void CheckUrlsFilename()
        {
            if (!(File.Exists(this.urlsFilename)))
            {
                File.CreateText(this.urlsFilename);
            }
        }

        public async Task LoadUrlsFromFileAsync()
        {
            List<string> urls = new List<string>();

            string url = string.Empty;

            using (StreamReader sr = new StreamReader(this.urlsFilename))
            {
                while ((url = await sr.ReadLineAsync()) != null)
                {
                    urls.Add(url);
                }
            }

            BuildStreamsCollection(urls);
        }

        private void BuildStreamsCollection(List<string> urls)
        {
            foreach (string url in urls)
            {
                StreamingService provider = DetermineStreamingService(url);
                StreamBase stream = null;

                switch (provider)
                {
                    case StreamingService.Twitch:
                        stream = new TwitchStream(url);
                        break;
                    case StreamingService.Ustream:
                        break;
                    case StreamingService.Justin:
                        stream = new JustinStream(url);
                        break;
                    case StreamingService.UnsupportedService:
                        break;
                    case StreamingService.None:
                        break;
                    default:
                        break;
                }

                this.Streams.Add(stream);
            }
        }

        private StreamingService DetermineStreamingService(string s)
        {
            StreamingService ss = StreamingService.None;

            Uri uri = new Uri(s);

            switch (uri.Host)
            {
                case "twitch.tv":
                    ss = StreamingService.Twitch;
                    break;
                case "ustream.tv":
                    ss = StreamingService.Ustream;
                    break;
                case "justin.tv":
                    ss = StreamingService.Justin;
                    break;
                default:
                    ss = StreamingService.UnsupportedService;
                    break;
            }

            return ss;
        }

        public async Task UpdateAllAsync()
        {
            VisualStateManager.GoToState(Application.Current.MainWindow, "Updating", false);

            foreach (StreamBase stream in this.Streams)
            {
                await stream.UpdateAsync();
            }

            VisualStateManager.GoToState(Application.Current.MainWindow, "Stable", false);
        }

        private void GoToStream(object parameter)
        {
            StreamBase stream = parameter as StreamBase;
            
            Misc.OpenUrlInBrowser(stream.Uri);
        }

        private void OpenFeedsFile(object parameter)
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

        private bool canExecuteCommand(object parameter)
        {
            // no need for any special logic, no reason to ever deny this
            return true;
        }
    }
}
