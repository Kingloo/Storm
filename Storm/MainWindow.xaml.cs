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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show("Do you really want to Quit?", "Quit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            switch (mbr)
            {
                case MessageBoxResult.Yes:
                    e.Cancel = false;
                    break;
                case MessageBoxResult.No:
                    e.Cancel = true;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
