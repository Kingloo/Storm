using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;
using StormDesktop.Interfaces;
using StormLib.Interfaces;
using StormLib.Services;

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

			CreateAndBindItemsControlSource(streamsItemsControl);

			viewModel.LoadStreamsCommand.Execute();

			viewModel.StartListeningToMessageQueue();
		}

		private void CreateAndBindItemsControlSource(ItemsControl itemsControl)
		{
			Binding binding = new Binding
			{
				Mode = BindingMode.OneTime,
				Source = CreateCollectionViewSource(viewModel.Streams)
			};
			
			BindingOperations.SetBinding(itemsControl, ItemsControl.ItemsSourceProperty, binding);
		}

		private static CollectionViewSource CreateCollectionViewSource(IReadOnlyCollection<IStream> source)
		{
			CollectionViewSource cvs = new CollectionViewSource
			{
				IsLiveSortingRequested = true,
				Source = source
			};

			ListCollectionView lcv = (ListCollectionView)cvs.View;
			
			lcv.LiveSortingProperties.Add(nameof(IStream.Status));
			lcv.LiveSortingProperties.Add(nameof(IStream.DisplayName));
			lcv.LiveSortingProperties.Add(nameof(IStream.ServiceName));

			lcv.CustomSort = Comparer<IStream>.Create(BaseStream.Comparer);

			return cvs;
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

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			logger.LogDebug("main window closed");
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
