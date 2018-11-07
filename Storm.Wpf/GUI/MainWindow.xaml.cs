using System;
using System.Diagnostics;
using System.Windows;
using Storm.Wpf.Common;
using Storm.Wpf.ViewModels;

namespace Storm.Wpf.GUI
{
    public partial class MainWindow : Window
    {
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
    }
}
