using System.Text;
using System.Windows;
using System.Windows.Input;
using Storm.Model;
using Storm.ViewModels;

namespace Storm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.WorkArea.Bottom - 200;

            DataContext = new MainWindowViewModel(this, ((App)App.Current).UrlsRepo);

            KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
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
