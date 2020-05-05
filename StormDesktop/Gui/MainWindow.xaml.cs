using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using StormDesktop.Interfaces;

namespace StormDesktop.Gui
{
    public partial class MainWindow : Window
    {
        private IntPtr hWnd = IntPtr.Zero;
        private readonly IMainWindowViewModel viewModel;

        public MainWindow(IMainWindowViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        private void mainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();

            SetMaxHeight();
        }

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await viewModel.LoadStreamsAsync();

            viewModel.StartUpdateTimer(TimeSpan.FromSeconds(90d));
        }

        private void mainWindow_LocationChanged(object sender, EventArgs e)
        {
            SetMaxHeight();
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            viewModel.StopUpdateTimer();
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            viewModel.CleanUp();
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
