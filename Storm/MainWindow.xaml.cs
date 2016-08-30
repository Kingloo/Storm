using System.Collections.Generic;
using System.Linq;
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

            MaxHeight = SystemParameters.WorkArea.Bottom - 100;

            DataContext = new MainWindowViewModel(this, ((App)App.Current).UrlsRepo);

#if DEBUG
            KeyUp += DEBUG_MainWindow_KeyUp;
#endif
        }

#if DEBUG

        private void DEBUG_MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                IEnumerable<StreamBase> s = from each in (DataContext as MainWindowViewModel).Streams
                                            where each.Uri.AbsoluteUri.Contains("unicorn")
                                            select each;

                foreach (StreamBase each in s)
                {
                    each.DEBUG_toggle_is_live();
                }
            }
        }
#endif
    }
}
