using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;
using StormDesktop.Interfaces;

namespace StormDesktop.Gui
{
	public partial class MainWindow : Window
	{
		private IntPtr hWnd = IntPtr.Zero;
		
		private readonly ILogger<MainWindow> logger;
		private readonly IMainWindowViewModel viewModel;

		public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel viewModel)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(viewModel);

			InitializeComponent();

			Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

			this.logger = logger;
			this.viewModel = viewModel;

			DataContext = this.viewModel;
		}

		private void MainWindow_SourceInitialized(object sender, EventArgs e)
		{
			hWnd = new WindowInteropHelper(this).EnsureHandle();

			SetMaxHeight();
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
            logger.LogDebug("main window loaded");

			viewModel.LoadStreamsCommand.Execute(null);

			viewModel.StartListeningToMessageQueue();
		}

		private void MainWindow_LocationChanged(object sender, EventArgs e)
		{
			SetMaxHeight();
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			logger.LogDebug("main window closing");

			viewModel.StopListeningToQueue();
		}

		private void SetMaxHeight()
		{
			var currentMonitor = System.Windows.Forms.Screen.FromHandle(hWnd);

			double height = currentMonitor?.WorkingArea.Bottom ?? SystemParameters.WorkArea.Bottom;
			double leeway = 100d;

			MaxHeight = height - leeway;
		}
	}
}
