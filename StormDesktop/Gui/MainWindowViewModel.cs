using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormDesktop.Common;
using StormDesktop.Interfaces;
using StormLib;
using StormLib.Helpers;
using StormLib.Interfaces;

namespace StormDesktop.Gui
{
	public class MainWindowViewModel : BindableBase, IMainWindowViewModel
	{
		private DelegateCommandAsync? _updateCommand;
		public DelegateCommandAsync UpdateCommand
		{
			get
			{
				_updateCommand ??= new DelegateCommandAsync(UpdateAsync, CanExecuteAsync);

				return _updateCommand;
			}
		}

		private DelegateCommandAsync? _loadStreamsCommand;
		public DelegateCommandAsync LoadStreamsCommand
		{
			get
			{
				_loadStreamsCommand ??= new DelegateCommandAsync(LoadStreamsAsync, CanExecuteAsync);

				return _loadStreamsCommand;
			}
		}

		private DelegateCommand<IStream>? _openPageCommand;
		public DelegateCommand<IStream> OpenPageCommand
		{
			get
			{
				_openPageCommand ??= new DelegateCommand<IStream>(OpenPage, (_) => true);

				return _openPageCommand;
			}
		}

		private DelegateCommand<IStream>? _openStreamCommand;
		public DelegateCommand<IStream> OpenStreamCommand
		{
			get
			{
				_openStreamCommand ??= new DelegateCommand<IStream>(OpenStream, (_) => true);

				return _openStreamCommand;
			}
		}

		private DelegateCommand? _openStreamsFileCommand;
		public DelegateCommand OpenStreamsFileCommand
		{
			get
			{
				_openStreamsFileCommand ??= new DelegateCommand(OpenStreamsFile, (_) => true);

				return _openStreamsFileCommand;
			}
		}

		private DelegateCommand<Window>? _exitCommand;
		public DelegateCommand<Window> ExitCommand
		{
			get
			{
				_exitCommand ??= new DelegateCommand<Window>(window => window.Close(), (_) => true);

				return _exitCommand;
			}
		}

		private bool CanExecuteAsync(object? _) => !IsActive;

		public void TriggerCanExecuteChanged()
		{
			UpdateCommand.RaiseCanExecuteChanged();
			LoadStreamsCommand.RaiseCanExecuteChanged();
			OpenPageCommand.RaiseCanExecuteChanged();
			OpenStreamCommand.RaiseCanExecuteChanged();
			OpenStreamsFileCommand.RaiseCanExecuteChanged();
		}

		private readonly ILogger<IMainWindowViewModel> logger;
		private readonly IOptionsMonitor<StormOptions> stormOptionsMonitor;

		private readonly ObservableCollection<IStream> streams = new ObservableCollection<IStream>();
		public IReadOnlyCollection<IStream> Streams { get => streams; }

		private bool isActive = false;
		public bool IsActive
		{
			get => isActive;
			set
			{
				SetProperty(ref isActive, value, nameof(IsActive));

				TriggerCanExecuteChanged();
			}
		}

		public MainWindowViewModel(ILogger<IMainWindowViewModel> logger, IOptionsMonitor<StormOptions> stormOptionsMonitor)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(stormOptionsMonitor);

			this.logger = logger;
			this.stormOptionsMonitor = stormOptionsMonitor;
		}

		public Task LoadStreamsAsync()
			=> LoadStreamsAsync(CancellationToken.None);

		public async Task LoadStreamsAsync(CancellationToken cancellationToken)
		{
			string[] lines = await FileSystem.LoadLinesFromFileAsync(stormOptionsMonitor.CurrentValue.StreamsFilePath).ConfigureAwait(true);

			if (lines.Length == 0)
            {
                return;
            }

			IReadOnlyCollection<IStream> loadedStreams = StreamFactory.CreateMany(lines, "#");

			var addedStreams = AddNew(loadedStreams);
			RemoveOld(loadedStreams);

			await UpdateAsync(addedStreams).ConfigureAwait(true);
		}

		private IReadOnlyCollection<IStream> AddNew(IReadOnlyCollection<IStream> loadedStreams)
		{
			Collection<IStream> addedStreams = new Collection<IStream>();

			foreach (IStream stream in loadedStreams)
			{
				if (!Streams.Contains(stream))
				{
					streams.Add(stream);

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
				streams.Remove(stream);
			}
		}

		private void UpdateTimer_Tick(object? sender, EventArgs e) => UpdateCommand.Execute(null);

		public Task UpdateAsync()
			=> UpdateAsync(Streams, CancellationToken.None);

		public Task UpdateAsync(CancellationToken cancellationToken)
			=> UpdateAsync(Streams, cancellationToken);

		public Task UpdateAsync(IEnumerable<IStream> streams)
			=> UpdateAsync(streams, CancellationToken.None);

		public async Task UpdateAsync(IEnumerable<IStream> streams, CancellationToken cancellationToken)
		{
			IsActive = true;

			IList<IStream> notLiveBeforeUpdate = Streams.Where(static s => s.Status != Status.Public).ToList();

			await servicesManager.UpdateAsync(streams).ConfigureAwait(true);

			IEnumerable<IStream> liveAfterUpdate = Streams.Where(static s => s.Status == Status.Public);

			IList<IStream> forWhichToNotify = notLiveBeforeUpdate.Intersect(liveAfterUpdate).ToList();

			SendNotifications(forWhichToNotify);

			IsActive = false;
		}

		private void SendNotifications(IEnumerable<IStream> forWhichToNotify)
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
				logger.LogWarning("failed to open URI: '{uri}'", stream.Link.AbsoluteUri);
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

				if (!SystemLaunch.Launch(pInfo))
				{
					logger.LogWarning("failed to launch streamlink for '{uri} ({service})'", stream.Name, stream.ServiceName);
				}
			}
			else
			{
				OpenPage(stream);
			}
		}

		private void OpenStreamsFile()
		{
			if (!SystemLaunch.Path(stormOptionsMonitor.CurrentValue.StreamsFilePath))
			{
				logger.LogWarning("failed to open file: '{file}'", stormOptionsMonitor.CurrentValue.StreamsFilePath);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(base.ToString());
			sb.AppendLine(CultureInfo.CurrentCulture, $"number of streams: {Streams.Count}");
			sb.AppendLine(CultureInfo.CurrentCulture, $"is active: {IsActive}");

			return sb.ToString();
		}
	}
}
