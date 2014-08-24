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
        #region Fields
        private readonly string urlsFilename = string.Format(@"C:\Users\{0}\Documents\StormUrls.txt", Environment.UserName);
        private DispatcherTimer updateTimer = new DispatcherTimer();
        #endregion

        #region Properties
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
                    this._openFeedsFileCommand = new DelegateCommand(OpenFeedsFile, canExecuteCommand);
                }

                return this._openFeedsFileCommand;
            }
        }
        #endregion

        public StreamManager()
        {
            this.updateTimer.Interval = new TimeSpan(0, 2, 30);
            this.updateTimer.Tick += async (sender, e) =>
            {
                await this.UpdateAllAsync();
            };

            this.updateTimer.IsEnabled = true;

            CheckUrlsFilename();
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
            string fileContents = string.Empty;
            List<string> urls = new List<string>();

            using (FileStream fsAsync = new FileStream(this.urlsFilename, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            {
                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    fileContents = await sr.ReadToEndAsync();
                }
            }

            using (StringReader sr = new StringReader(fileContents))
            {
                string each = string.Empty;

                while ((each = await sr.ReadLineAsync()) != null)
                {
                    AddService(each);
                }
            }
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

        public async Task UpdateAllAsync()
        {
            VisualStateManager.GoToState(Application.Current.MainWindow, "Updating", false);

            foreach (StreamBase stream in this.Streams)
            {
                await stream.UpdateAsync();
            }

            VisualStateManager.GoToState(Application.Current.MainWindow, "Stable", false);
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
