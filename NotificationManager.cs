using System;
using System.Collections.Generic;

namespace Storm
{
    class NotificationManager
    {
        List<NotificationWindow> notificationWindows = new List<NotificationWindow>();

        public NotificationManager() { }

        public void CreateNotification(string text, TimeSpan duration, string url)
        {
            NotificationWindow nw = new NotificationWindow(text, duration, url);
            nw.Closed += nw_Closed;
            this.notificationWindows.Add(nw);
            nw.Show();
            System.Media.SystemSounds.Hand.Play();
        }

        void nw_Closed(object sender, EventArgs e)
        {
            notificationWindows.Remove(sender as NotificationWindow);
        }

        public void KillAllNotifications()
        {
            foreach (NotificationWindow window in notificationWindows)
            {
                window.Close();
            }

            notificationWindows.Clear();
        }
    }
}
