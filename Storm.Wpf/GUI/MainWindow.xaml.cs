using System;
using System.Windows;
using System.Windows.Interop;
using Storm.Wpf.Common;
using Storm.Wpf.ViewModels;

namespace Storm.Wpf.GUI
{
    public partial class MainWindow : Window
    {
        private IntPtr hWnd = IntPtr.Zero;
        private readonly MainWindowViewModel viewModel = null;

        public MainWindow(FileLoader fileLoader)
        {
            if (fileLoader is null) { throw new ArgumentNullException(nameof(fileLoader)); }

            InitializeComponent();

            viewModel = new MainWindowViewModel(fileLoader);

            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.LoadStreamsCommand.Execute(null);

            viewModel.StartUpdateTimer();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            viewModel.StopUpdateTimer();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();

            SetMaxHeight();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SetMaxHeight();
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
