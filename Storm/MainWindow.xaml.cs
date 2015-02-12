using System.Windows;

namespace Storm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.MaxHeight = CalculateMaxHeight();
        }

        private double CalculateMaxHeight()
        {
            double screenHeight = SystemParameters.WorkArea.Bottom;
            double maxHeight = screenHeight - 150;

            return maxHeight;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await streamManager.UpdateAllAsync();
        }
    }
}
