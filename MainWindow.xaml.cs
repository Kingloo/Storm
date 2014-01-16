using System;
using System.Windows;
using System.Windows.Controls;

namespace Storm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await streamManager.LoadUrlsFromFileAsync();
            await streamManager.UpdateAllAsync();
        }
    }
}
