using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Storm
{
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer closeTimer = new DispatcherTimer();
        private string doubleClickUrl = string.Empty;

        public NotificationWindow(string text, TimeSpan duration, string url)
        {
            this.InitializeComponent();

            this.closeTimer.Interval = duration;
            this.closeTimer.Tick += closeTimer_Tick;
            this.closeTimer.IsEnabled = true;

            this.lbl_Notify.Content = text;
            this.doubleClickUrl = url;
        }

        void closeTimer_Tick(object sender, EventArgs e)
        {
            this.closeTimer.Tick -= closeTimer_Tick;
            this.closeTimer.IsEnabled = false;
            this.Close();
        }

        private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Misc.OpenUrlInBrowser(this.doubleClickUrl);
        }
    }
}
