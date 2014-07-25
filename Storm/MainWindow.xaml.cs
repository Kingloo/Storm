using System;
using System.Threading.Tasks;
using System.Windows;

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
            StreamManager sm = (StreamManager)Application.Current.Resources["streamManager"];

            await sm.UpdateAllAsync();
        }
    }
}
