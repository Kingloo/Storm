using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using StormDesktop.Common;
using StormDesktop.Interfaces;
using StormLib;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Services;
using StormLib.Streams;

namespace StormDesktop.Gui
{
    public class MainWindowViewModel : BindableBase, IMainWindowViewModel
    {
        private readonly ILog logger;
        private readonly IServicesManager servicesManager;
        private readonly string filePath = string.Empty;
        
        private DispatcherTimer? updateTimer = null;

        private readonly ObservableCollection<IStream> _streams = new ObservableCollection<IStream>();
        public IReadOnlyCollection<IStream> Streams => _streams;

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, nameof(IsActive));
        }

        private DelegateCommandAsync? _updateCommand;
        public DelegateCommandAsync UpdateCommand
        {
            get
            {
                if (_updateCommand is null)
                {
                    _updateCommand = new DelegateCommandAsync(UpdateAsync, CanExecuteAsync);
                }

                return _updateCommand;
            }
        }

        private DelegateCommandAsync? _loadStreamsCommand;
        public DelegateCommandAsync LoadStreamsCommand
        {
            get
            {
                if (_loadStreamsCommand is null)
                {
                    _loadStreamsCommand = new DelegateCommandAsync(LoadStreamsAsync, CanExecuteAsync);
                }

                return _loadStreamsCommand;
            }
        }

        private DelegateCommand<IStream>? _openPageCommand;
        public DelegateCommand<IStream> OpenPageCommand
        {
            get
            {
                if (_openPageCommand is null)
                {
                    _openPageCommand = new DelegateCommand<IStream>(OpenPage, (_) => true);
                }

                return _openPageCommand;
            }
        }

        private DelegateCommand<IStream>? _openStreamCommand;
        public DelegateCommand<IStream> OpenStreamCommand
        {
            get
            {
                if (_openStreamCommand is null)
                {
                    _openStreamCommand = new DelegateCommand<IStream>(OpenStream, (_) => true);
                }

                return _openStreamCommand;
            }
        }

        private DelegateCommand? _openStreamsFileCommand;
        public DelegateCommand OpenStreamsFileCommand
        {
            get
            {
                if (_openStreamsFileCommand is null)
                {
                    _openStreamsFileCommand = new DelegateCommand(OpenStreamsFile, (_) => true);
                }

                return _openStreamsFileCommand;
            }
        }

        private DelegateCommand<TwitchStream>? _openTwitchPlayerCommand;
        public DelegateCommand<TwitchStream> OpenTwitchPlayerCommand
        {
            get
            {
                if (_openTwitchPlayerCommand is null)
                {
                    _openTwitchPlayerCommand = new DelegateCommand<TwitchStream>(OpenTwitchPlayer, (_) => true);
                }

                return _openTwitchPlayerCommand;
            }
        }

        private DelegateCommand<Window>? _exitCommand;
        public DelegateCommand<Window> ExitCommand
        {
            get
            {
                if (_exitCommand is null)
                {
                    _exitCommand = new DelegateCommand<Window>(window => window.Close(), (_) => true);
                }

                return _exitCommand;
            }
        }

        private bool CanExecuteAsync(object _) => !IsActive;

        public MainWindowViewModel(ILog logger, IServicesManager servicesManager, string filePath)
        {
            this.logger = logger;
            this.servicesManager = servicesManager;
            this.filePath = filePath;
        }

        public async Task LoadStreamsAsync()
        {
            string[] lines = await FileSystem.ReadAllLinesAsync(filePath);

            if (lines.Length == 0) { return; }

            IReadOnlyCollection<IStream> loadedStreams = StreamFactory.CreateMany(lines, "#");

            var addedStreams = AddNew(loadedStreams);
            RemoveOld(loadedStreams);

            await UpdateAsync(addedStreams);
        }

        private IReadOnlyCollection<IStream> AddNew(IReadOnlyCollection<IStream> loadedStreams)
        {
            Collection<IStream> addedStreams = new Collection<IStream>();

            foreach (IStream stream in loadedStreams)
            {
                if (!Streams.Contains(stream))
                {
                    _streams.Add(stream);
                    
                    addedStreams.Add(stream);
                }
            }

            return addedStreams;
        }

        private void RemoveOld(IReadOnlyCollection<IStream> loadedStreams)
        {
            var toRemove = Streams
                .Where(s => !loadedStreams.Contains(s))
                .ToList();

            foreach (IStream stream in toRemove)
            {
                _streams.Remove(stream);
            }
        }

        public void StartUpdateTimer(TimeSpan updateFrequency)
        {
            if (updateTimer is null)
            {
                updateTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
                {
                    Interval = updateFrequency
                };

                updateTimer.Tick += UpdateTimer_Tick;

                updateTimer.Start();
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e) => UpdateCommand.Execute(null);

        public void StopUpdateTimer()
        {
            if (!(updateTimer is null))
            {
                updateTimer.Stop();
                updateTimer.Tick -= UpdateTimer_Tick;

                updateTimer = null;
            }
        }

        public Task UpdateAsync() => UpdateAsync(Streams);

        public async Task UpdateAsync(IEnumerable<IStream> streams)
        {
            IsActive = true;

            var notLiveBeforeUpdate = Streams.Where(s => s.Status != Status.Public).ToList();

            await servicesManager.UpdateAsync(streams);

            var liveAfterUpdate = Streams.Where(s => s.Status == Status.Public);

            var forWhichToNotify = notLiveBeforeUpdate.Intersect(liveAfterUpdate).ToList();

            SendNotifications(forWhichToNotify);

            IsActive = false;
        }

        private void SendNotifications(List<IStream> forWhichToNotify)
        {
            foreach (IStream toNotify in forWhichToNotify)
            {
                string title = $"{toNotify.DisplayName} is LIVE";
                string description = $"on {toNotify.ServiceName}";
                void notify() => OpenStream(toNotify);

                NotificationService.Send(title, description, notify);
            }
        }

        private void OpenPage(IStream stream)
        {
            if (!SystemLaunch.Uri(stream.Link))
            {
                logger.Message($"{stream.Link.AbsoluteUri} could not be opened", Severity.Error);
            }
        }

        private void OpenStream(IStream stream)
        {
            if (stream.HasStreamlinkSupport)
            {
                string command = string.Format(CultureInfo.CurrentCulture, "/C streamlink {0} best", stream.Link);

                ProcessStartInfo pInfo = new ProcessStartInfo()
                {
                    Arguments = command,
                    ErrorDialog = true, // maybe remove
                    FileName = "powershell.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                if (!SystemLaunch.Process(pInfo))
                {
                    string message = $"launching streamlink for {stream.Name} failed!";

                    logger.Message(message, Severity.Error);
                }
            }
            else
            {
                OpenPage(stream);
            }
        }

        private void OpenStreamsFile()
        {
            if (!SystemLaunch.Path(filePath))
            {
                logger.Message($"file could not be opened: {filePath}", Severity.Error);
            }
        }

        private void OpenTwitchPlayer(TwitchStream stream)
        {
            Uri player = TwitchService.GetPlayerUriForStream(stream);

            if (!SystemLaunch.Uri(player))
            {
                logger.Message($"{player.AbsoluteUri} could not be opened", Severity.Error);
            }
        }

        public void CleanUp()
        {
            servicesManager.Dispose();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(base.ToString());
            sb.AppendLine($"number of streams: {Streams.Count}");
            sb.AppendLine($"is active: {IsActive}");

            sb.AppendLine("services:");

            if (servicesManager.Services.Count > 0)
            {
                foreach (IService service in servicesManager.Services)
                {
                    sb.Append(service.ToString());
                }
            }
            else
            {
                sb.AppendLine("no services registered");
            }
            
            return sb.ToString();
        }        
    }
}
