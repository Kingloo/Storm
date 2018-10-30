using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Storm.Wpf.Common;
using Storm.Wpf.ViewModels;

namespace Storm.Wpf.GUI
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel = null;

        public MainWindow(FileLoader fileLoader)
        {
            InitializeComponent();

            if (fileLoader is null) { throw new ArgumentNullException(nameof(fileLoader)); }

            viewModel = new MainWindowViewModel(fileLoader);

            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
