using System.Text;
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

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                StringBuilder sb = new StringBuilder();

                foreach (StreamBase each in streamManager.Streams)
                {
                    sb.AppendLine(each.ToString());
                }

                Utils.LogMessage(sb.ToString());
            }
        }
    }
}
