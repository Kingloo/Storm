using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;
using Storm.Model;
using Storm.ViewModels;

namespace Storm
{
    public partial class MainWindow : Window
    {
        private IntPtr hWnd = IntPtr.Zero;
        private readonly MainWindowViewModel vm = null;

        public MainWindow()
        {
            InitializeComponent();

            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += MainWindow_LocationChanged;
            Loaded += MainWindow_Loaded;

            vm = new MainWindowViewModel(this, ((App)App.Current).UrlsRepo);
            DataContext = vm;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();

            CalculateMaxHeight();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            CalculateMaxHeight();
        }

        private void CalculateMaxHeight()
        {
            Screen currentMonitor = Screen.FromHandle(hWnd);

            MaxHeight = currentMonitor == null
                ? SystemParameters.WorkArea.Bottom - 100
                : currentMonitor.WorkingArea.Bottom - 100;
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await vm.LoadUrlsAsync();
            
            await vm.UpdateAsync();
        }
        
        private void go_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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
