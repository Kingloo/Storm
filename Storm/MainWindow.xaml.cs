using System.Text;
using System.Windows;
using Storm.Model;
using Storm.ViewModels;

namespace Storm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(this, ((App)App.Current).UrlsRepo);

            MaxHeight = CalculateMaxHeight();
        }

        private static double CalculateMaxHeight()
        {
            double screenHeight = SystemParameters.WorkArea.Bottom;
            double maxHeight = screenHeight - 150;

            return maxHeight;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                StringBuilder sb = new StringBuilder();

                foreach (StreamBase each in ((MainWindowViewModel)DataContext).Streams)
                {
                    sb.AppendLine(each.ToString());
                }

                Utils.LogMessage(sb.ToString());
            }
        }
    }
}
