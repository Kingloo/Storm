using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using StormDesktop.Common;
using StormLib.Helpers;
using StormLib.Interfaces;

namespace StormDesktop.Gui
{
    public class MainWindowViewModel : BindableBase, IMainWindowViewModel
    {
        private readonly ILogClass logger;
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

        public MainWindowViewModel(ILogClass logger, IServicesManager servicesManager, string filePath)
        {
            this.logger = logger;
            this.servicesManager = servicesManager;
            this.filePath = filePath;
        }

        public async Task LoadStreamsAsync()
        {
            string[] lines = await FileSystem.ReadAllLinesAsync(filePath);

            if (lines.Length == 0) { return; }

            IReadOnlyCollection<IStream> streams = StreamFactory.CreateMany(lines, "#");

            AddNew(streams);
            RemoveOld(streams);
        }

        private void AddNew(IReadOnlyCollection<IStream> streams)
        {
            foreach (IStream stream in streams)
            {
                if (!Streams.Contains(stream))
                {
                    _streams.Add(stream);
                }
            }
        }

        private void RemoveOld(IReadOnlyCollection<IStream> streams)
        {
            var toRemove = streams
                .Where(s => !Streams.Contains(s))
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

            await servicesManager.UpdateAsync(streams);

            IsActive = false;
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

        }

        private void OpenStreamsFile()
        {
            if (!SystemLaunch.Path(filePath))
            {
                logger.Message($"file could not be opened: {filePath}", Severity.Error);
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
