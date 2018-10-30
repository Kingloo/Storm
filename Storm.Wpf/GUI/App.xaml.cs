using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Storm.Wpf.Common;

namespace Storm.Wpf.GUI
{
    public partial class App : Application
    {
        public App(FileLoader fileLoader)
        {
            InitializeComponent();

            MainWindow = new MainWindow(fileLoader);

            MainWindow.Show();
        }
    }
}
