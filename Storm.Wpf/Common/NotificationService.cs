using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Storm.Wpf.Common
{
    public static class NotificationService
    {
        private static bool canShowNotification = true;

        private readonly static Queue<Notification> notificationQueue = new Queue<Notification>();

        private static DispatcherTimer queuePullTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(2d)
        };

        public static void Send(string title) => Send(title, string.Empty, null);

        public static void Send(string title, string description) => Send(title, description, null);

        public static void Send(string title, string description, Action action)
        {
            InitTimer();

            Debug.WriteLine($"send: {title}, {description}, {(action == null ? "no action" : "an action")}");

            Notification notification = new Notification(title, description, action);

            notification.Closed += (s, e) => canShowNotification = true;

            notificationQueue.Enqueue(notification);
        }

        private static void InitTimer()
        {
            queuePullTimer.Tick += QueuePullTimer_Tick;

            queuePullTimer.Start();
        }

        private static void QueuePullTimer_Tick(object sender, EventArgs e)
        {
            if (notificationQueue.Count > 0 && canShowNotification)
            {
                Notification nextNotification = notificationQueue.Dequeue();

                Display(nextNotification);

                canShowNotification = false;
            }
        }

        private static void Display(Notification notification)
        {
            if (notification is null) { throw new ArgumentNullException(nameof(notification)); }

            notification.Show();

            System.Media.SystemSounds.Hand.Play();
        }



        private sealed class Notification : Window
        {
            private DispatcherCountdownTimer closeTimer = null;

            public string NotificationTitle { get; } = "untitled";
            public string Description { get; } = string.Empty;
            public Action Action { get; } = null;

            public Notification(string title, string description, Action action)
            {
                NotificationTitle = title;
                Description = description;
                Action = action;

                Width = 350d;
                Height = 250d;

                Left = 200d;
                Top = 200d;

                closeTimer = new DispatcherCountdownTimer(TimeSpan.FromSeconds(4d), Close);

                Loaded += (s, e) => closeTimer.Start();
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(GetType().FullName);
                sb.AppendLine(NotificationTitle);
                sb.AppendLine(Description);
                sb.AppendLine(Action == null ? "no action" : $"action method: {Action.Method.Name}");
                sb.AppendLine(IsActive ? "window active" : "window inactive");
                sb.AppendLine(IsLoaded ? "window loaded" : "window not loaded");
                sb.AppendLine(IsVisible ? "window visible" : "window not visible");

                return sb.ToString();
            }
        }
    }
}
