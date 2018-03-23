using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;
using Storm.Model;
using Storm.ViewModels;

namespace Storm.Views
{
    public partial class MainWindow : Window
    {
        #region Fields
        private IntPtr hWnd = IntPtr.Zero;

        private MainWindowViewModel vm = default;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContextChanged += MainWindow_DataContextChanged;
            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += MainWindow_LocationChanged;
            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            vm = (MainWindowViewModel)args.NewValue;

            vm.StatusChanged += (s, e) => VisualStateManager.GoToState(this, e.IsUpdating ? "Updating" : "Stable", false);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hWnd = new WindowInteropHelper(this).EnsureHandle();

            CalculateMaxHeight();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) => CalculateMaxHeight();

        private void CalculateMaxHeight()
        {
            var currentMonitor = System.Windows.Forms.Screen.FromHandle(hWnd);

            double height = currentMonitor?.WorkingArea.Bottom ?? SystemParameters.WorkArea.Bottom;
            double leeway = 100d;

            MaxHeight = height - leeway;
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e) => await vm.LoadUrlsAsync();

        private void SelectorMovement_KeyUp(object sender, KeyEventArgs e)
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

        private void OpenStream(object sender)
        {
            var stream = (StreamBase)(((Rectangle)sender).DataContext);

            vm.GoToStream(stream);
        }
    }
}
