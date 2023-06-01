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
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StormDesktop.Common;
using StormDesktop.Interfaces;
using StormLib;
using StormLib.Helpers;
using StormLib.Interfaces;
using StormLib.Services.Chaturbate;
using StormLib.Services.Kick;
using StormLib.Services.Mixlr;
using StormLib.Services.Rumble;
using StormLib.Services.Twitch;
using StormLib.Services.YouTube;

namespace StormDesktop.Gui
{
	public class MainWindowViewModel : BindableBase, IMainWindowViewModel, IDisposable
	{
		private DelegateCommandAsync<IStream>? _updateCommand;
		public DelegateCommandAsync<IStream> UpdateCommand
		{
			get
			{
				_updateCommand ??= new DelegateCommandAsync<IStream>(UpdateAsync, CanExecuteAsync);

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
				_openPageCommand ??= new DelegateCommand<IStream>(OpenPage);

				return _openPageCommand;
			}
		}

		private DelegateCommand<IStream>? _openStreamCommand;
		public DelegateCommand<IStream> OpenStreamCommand
		{
			get
			{
				_openStreamCommand ??= new DelegateCommand<IStream>(OpenStream);

				return _openStreamCommand;
			}
		}

		private DelegateCommand? _openStreamsFileCommand;
		public DelegateCommand OpenStreamsFileCommand
		{
			get
			{
				_openStreamsFileCommand ??= new DelegateCommand(OpenStreamsFile);

				return _openStreamsFileCommand;
			}
		}

		private DelegateCommand<Window>? _exitCommand;
		public DelegateCommand<Window> ExitCommand
		{
			get
			{
				_exitCommand ??= new DelegateCommand<Window>(static window => window.Close());

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
		private readonly UpdaterMessageQueue updaterMessageQueue;

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

		private CancellationTokenSource? listenToMessageQueueCts = null;

		public MainWindowViewModel(
			ILogger<IMainWindowViewModel> logger,
			IOptionsMonitor<StormOptions> stormOptionsMonitor,
			UpdaterMessageQueue updaterMessageQueue)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(stormOptionsMonitor);
			ArgumentNullException.ThrowIfNull(updaterMessageQueue);

			this.logger = logger;
			this.stormOptionsMonitor = stormOptionsMonitor;
			this.updaterMessageQueue = updaterMessageQueue;

			updaterMessageQueue.SetStreamSource(Streams);
		}

		public void StartListeningToMessageQueue()
		{
			if (listenToMessageQueueCts is null)
			{
				listenToMessageQueueCts = new CancellationTokenSource();

				Task _ = Task.Run(ListenToMessageQueueAsync, listenToMessageQueueCts.Token);

				logger.LogDebug("listening to update message queue");
			}
		}

		private async ValueTask ListenToMessageQueueAsync()
		{
			while (listenToMessageQueueCts is not null
				&& listenToMessageQueueCts.IsCancellationRequested == false)
			{
				IList<IStream> notLiveBeforeUpdate = Streams.Where(s => s.Status != Status.Public).ToList();

				while (updaterMessageQueue.ResultsQueue.TryDequeue(out object? result))
				{
					PerformResultAction(result);
				}

				IEnumerable<IStream> liveAfterUpdate = Streams.Where(s => s.Status == Status.Public);

				IList<IStream> toSendNotificationsFor = notLiveBeforeUpdate.Intersect(liveAfterUpdate).ToList();

				SendNotifications(toSendNotificationsFor);

				await Task.Delay(TimeSpan.FromSeconds(3d)).ConfigureAwait(true);
			}

			logger.LogDebug("message queue listening outer loop stopped");
		}

		private static void PerformResultAction(object result)
		{
			switch (result)
			{
				case Result<ChaturbateStream> chaturbateResult:
					chaturbateResult.Action.Invoke(chaturbateResult.Stream);
					break;
				case Result<KickStream> kickResult:
					kickResult.Action.Invoke(kickResult.Stream);
					break;
				case Result<MixlrStream> mixlrResult:
					mixlrResult.Action.Invoke(mixlrResult.Stream);
					break;
				case Result<RumbleStream> rumbleResult:
					rumbleResult.Action.Invoke(rumbleResult.Stream);
					break;
				case Result<TwitchStream> twitchResult:
					twitchResult.Action.Invoke(twitchResult.Stream);
					break;
				case Result<YouTubeStream> youTubeResult:
					youTubeResult.Action.Invoke(youTubeResult.Stream);
					break;
				default:
					break;
			}
		}

		public void StopListeningToQueue()
		{
			if (listenToMessageQueueCts is not null)
			{
				listenToMessageQueueCts.Cancel();

				listenToMessageQueueCts.Dispose();

				listenToMessageQueueCts = null;
			}
		}

		public Task LoadStreamsAsync()
			=> LoadStreamsAsync(CancellationToken.None);

		public async Task LoadStreamsAsync(CancellationToken cancellationToken)
		{
			IReadOnlyList<string> lines = await FileSystem
				.LoadLinesFromFileAsync(stormOptionsMonitor.CurrentValue.StreamsFilePath)
				.ConfigureAwait(true);

			if (!lines.Any())
			{
				return;
			}

			IReadOnlyList<IStream> loadedStreams = StreamFactory.CreateMany(lines);

			AddNew(loadedStreams);

			RemoveOld(loadedStreams);
		}

		private void AddNew(IEnumerable<IStream> loadedStreams)
		{
			foreach (IStream stream in loadedStreams)
			{
				if (!Streams.Contains(stream))
				{
					streams.Add(stream);
				}
			}
		}

		private void RemoveOld(IEnumerable<IStream> loadedStreams)
		{
			var toRemove = Streams
				.Where(s => !loadedStreams.Contains(s))
				.ToList();

			foreach (IStream stream in toRemove)
			{
				streams.Remove(stream);
			}
		}

		private void SendNotifications(IEnumerable<IStream> forWhichToNotify)
		{
			foreach (IStream toNotify in forWhichToNotify)
			{
				string title = $"{toNotify.DisplayName} is LIVE";
				string description = $"on {toNotify.ServiceName}";
				void notify() => OpenStream(toNotify);

				Application.Current.Dispatcher.Invoke(
					() => NotificationService.Send(title, description, notify),
					DispatcherPriority.ApplicationIdle);
			}
		}

		private async Task UpdateAsync(IStream stream)
		{
			var updateTask = stream switch
			{
				ChaturbateStream c => updaterMessageQueue.UpdateAsync(new[] { c }, CancellationToken.None),
				KickStream k => updaterMessageQueue.UpdateAsync(new[] { k }, CancellationToken.None),
				MixlrStream m => updaterMessageQueue.UpdateAsync(new[] { m }, CancellationToken.None),
				RumbleStream r => updaterMessageQueue.UpdateAsync(new[] { r }, CancellationToken.None),
				TwitchStream t => updaterMessageQueue.UpdateAsync(new[] { t }, CancellationToken.None),
				YouTubeStream y => updaterMessageQueue.UpdateAsync(new[] { y }, CancellationToken.None),
				_ => throw new InvalidCastException($"bad stream type: '{stream.GetType().Name}'")
			};

			await updateTask.ConfigureAwait(false);
		}

		private void OpenStream(IStream stream)
		{
			if (stream.HasStreamlinkSupport)
			{
				if (!OpenWithStreamlink(stream, stormOptionsMonitor.CurrentValue.StreamlinkCommandFormat))
				{
					logger.LogWarning("failed to launch streamlink for '{Uri} ({Service})'", stream.Name, stream.ServiceName);
				}
			}
			else
			{
				if (!OpenWithBrowser(stream))
				{
					logger.LogWarning("failed to open URI: '{Uri}'", stream.Link.AbsoluteUri);
				}
			}
		}

		private static bool OpenWithStreamlink(IStream stream, string streamlinkFormat)
		{
			string command = string.Format(CultureInfo.CurrentCulture, streamlinkFormat, stream.Link);

			ProcessStartInfo pInfo = new ProcessStartInfo()
			{
				Arguments = command,
				ErrorDialog = true, // maybe remove
				FileName = "powershell.exe",
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = true
			};

			return SystemLaunch.Launch(pInfo);
		}

		private static bool OpenWithBrowser(IStream stream)
		{
			return SystemLaunch.Uri(stream.GetBrowserLink());
		}

		private void OpenPage(IStream stream)
		{
			OpenWithBrowser(stream);
		}

		private void OpenStreamsFile()
		{
			if (!SystemLaunch.Path(stormOptionsMonitor.CurrentValue.StreamsFilePath))
			{
				logger.LogWarning("failed to open file: '{File}'", stormOptionsMonitor.CurrentValue.StreamsFilePath);
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

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (listenToMessageQueueCts is not null)
					{
						listenToMessageQueueCts.Cancel();

						listenToMessageQueueCts.Dispose();

						listenToMessageQueueCts = null;
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);

			GC.SuppressFinalize(this);
		}
	}
}
