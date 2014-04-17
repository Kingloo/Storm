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

        private void MouseBinding_Changed(object sender, EventArgs e)
        {
            Console.WriteLine("HERE: " + e.ToString() + ", " + sender.ToString());
        }
    }
}
