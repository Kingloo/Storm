using System.Windows;

namespace Storm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            DataContext = new StreamManager(this);
        }
    }
}
