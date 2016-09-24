using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using Storm.Model;
using Storm.ViewModels;

namespace Storm
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel vm = null;

        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.WorkArea.Bottom - 100;

            vm = new MainWindowViewModel(this, ((App)App.Current).UrlsRepo);
            DataContext = vm;
            
            Loaded += MainWindow_Loaded;

#if DEBUG
            KeyUp += (sender, e) =>
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
            };
#endif
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await vm.LoadUrlsAsync();
            
            await vm.UpdateAsync();
        }
        
        private void go_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    break;
                case Key.Down:
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    break;
                case Key.Enter:
                    OpenStream(sender);
                    break;
                default:
                    break;
            }
        }

        private static void OpenStream(object sender)
        {
            // when "as" fails, it returns null
            // when casting fails it throws an exception
            // we prefer the latter

            Rectangle rect = (Rectangle)sender;
            StreamBase sb = (StreamBase)rect.DataContext;

            sb.GoToStream();
        }
    }
}
