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

        private readonly MainWindowViewModel vm = null;
        #endregion

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            vm = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            vm.StatusChanged += Vm_StatusChanged;

            DataContext = vm;

            SourceInitialized += MainWindow_SourceInitialized;
            LocationChanged += MainWindow_LocationChanged;
            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                default:
                    break;
            }
        }
        
        private void Vm_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, e.IsUpdating ? "Updating" : "Stable", false);
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

            MaxHeight = (currentMonitor?.WorkingArea.Bottom ?? SystemParameters.WorkArea.Bottom) - 100d;
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
