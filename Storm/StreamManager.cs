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
        private NotificationManager notificationManager = new NotificationManager();
        #endregion

        #region Visible
        public ObservableCollection<StreamBase> Streams { get; private set; }
        #endregion

        #region Commands
        private DelegateCommand _goToStreamCommand = null;
        public DelegateCommand GoToStreamCommand
        {
            get
            {
                if (this._goToStreamCommand == null)
                {
                    this._goToStreamCommand = new DelegateCommand(GoToStream, canExecuteCommand);
                }

                return this._goToStreamCommand;
            }
        }
        private DelegateCommand _openFeedsFileCommand = null;
        public DelegateCommand OpenFeedsFileCommand
        {
            get
            {
                if (this._openFeedsFileCommand == null)
                {
                    this._openFeedsFileCommand = new DelegateCommand(OpenFeedsFile, canExecuteCommand);
                }

                return this._openFeedsFileCommand;
            }
        }
        #endregion

        public StreamManager()
        {
            if (!(File.Exists(this.urlsFilename)))
            {
                File.CreateText(this.urlsFilename);
            }

            this.Streams = new ObservableCollection<StreamBase>();

            this.updateTimer.Tick += updateTimer_Tick;
            this.updateTimer.Interval = new TimeSpan(0, 2, 30);
            this.updateTimer.IsEnabled = true;
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            await this.UpdateAllAsync();
        }

        public Task LoadUrlsFromFileAsync()
        {
            return Task.Factory.StartNew(loadUrlsFromFileAsyncAction);
        }

        private void loadUrlsFromFileAsyncAction()
        {
            List<string> urls = new List<string>();

            string url = string.Empty;

            using (StreamReader sr = new StreamReader(this.urlsFilename))
            {
                while ((url = sr.ReadLine()) != null)
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
                        break;
                    case StreamingService.UnsupportedService:
                        break;
                    case StreamingService.None:
                        break;
                    default:
                        break;
                }

                Disp.Invoke(new Action(
                    delegate()
                    {
                        this.Streams.Add(stream);
                    }));
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

        public Task UpdateAllAsync()
        {
            VisualStateManager.GoToState(Application.Current.MainWindow, "Updating", false);

            return Task.Factory.StartNew(new Action(
                delegate()
                {
                    foreach (StreamBase stream in this.Streams)
                    {
                        stream.Update();
                    }

                    Disp.Invoke(new Action(
                        delegate()
                        {
                            VisualStateManager.GoToState(Application.Current.MainWindow, "Stable", false);
                        }));
                }));
        }

        private void GoToStream(object parameter)
        {
            if (!(parameter is StreamBase))
            {
                throw new ArgumentException("StreamManager.cs -> GoToStream(object parameter) -> parameter must be StreamBase");
            }

            StreamBase stream = parameter as StreamBase;

            try
            {
                Process.Start(stream.Uri);
            }
            catch (FileNotFoundException)
            {
                Process.Start("iexplore.exe", stream.Uri);
            }
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
            return true;
        }
    }
}
