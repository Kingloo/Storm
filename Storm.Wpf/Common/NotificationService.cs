using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Storm.Wpf.Common
{
    public static class NotificationService
    {
        private static bool canShowNotification = true;

        private readonly static Queue<Notification> notificationQueue = new Queue<Notification>();

        private static int timerTickCount = 0;
        private static int timerTickMax = 15;

        private static DispatcherTimer queuePullTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(3d)
        };



        public static void Send(string title) => Send(title, string.Empty, null);

        public static void Send(string title, string description) => Send(title, description, null);

        public static void Send(string title, Action action) => Send(title, string.Empty, action);

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

            if (!queuePullTimer.IsEnabled)
            {
                queuePullTimer.Start();
            }
        }

        private static void QueuePullTimer_Tick(object sender, EventArgs e)
        {
            if (notificationQueue.Count > 0)
            {
                if (canShowNotification)
                {
                    Notification nextNotification = notificationQueue.Dequeue();

                    Display(nextNotification);

                    canShowNotification = false;
                }
            }
            else
            {
                timerTickCount++; // incr the number of times the timer.Tick found nothing to work on
            }

            if (timerTickCount > timerTickMax)
            // if there was nothing to pop for x number of ticks, we turn the timer off
            {
                queuePullTimer.Stop();

                timerTickCount = 0; // and reset the counter
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
            private readonly DispatcherCountdownTimer closeTimer = null;

            internal Notification(string title, string description, Action action)
            {
                Style = BuildWindowStyle(action);

                bool hasDescription = !String.IsNullOrWhiteSpace(description);

                Grid grid = hasDescription ? BuildGrid(numRows: 2) : BuildGrid(numRows: 1);

                Label titleLabel = BuildLabel(BuildTitleLabelStyle(), title, FontStyles.Normal);

                Grid.SetRow(titleLabel, 0);
                grid.Children.Add(titleLabel);

                if (hasDescription)
                {
                    Label descriptionLabel = BuildLabel(BuildDescriptionLabelStyle(), description, FontStyles.Italic);
                    Grid.SetRow(descriptionLabel, 1);
                    grid.Children.Add(descriptionLabel);
                }

                AddChild(grid);

#if DEBUG
                TimeSpan windowCloseInterval = TimeSpan.FromSeconds(4d);
#else
                TimeSpan windowCloseInterval = TimeSpan.FromSeconds(15d);
#endif

                closeTimer = new DispatcherCountdownTimer(windowCloseInterval, Close);

                Loaded += (s, e) => closeTimer.Start();
                Closing += (s, e) => closeTimer.Stop();
            }

            private static Style BuildWindowStyle(Action action)
            {
                Style style = new Style(typeof(Notification));

                if (action != null)
                {
                    var handler = new MouseButtonEventHandler((s, e) => action());
                    var leftMouseDoubleClick = new EventSetter(MouseDoubleClickEvent, handler);

                    style.Setters.Add(leftMouseDoubleClick);
                }

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.Transparent));

                style.Setters.Add(new Setter(TopmostProperty, true));
                style.Setters.Add(new Setter(FocusableProperty, false));
                style.Setters.Add(new Setter(ShowInTaskbarProperty, false));
                style.Setters.Add(new Setter(ShowActivatedProperty, false));
                style.Setters.Add(new Setter(IsTabStopProperty, false));
                style.Setters.Add(new Setter(ResizeModeProperty, ResizeMode.NoResize));
                style.Setters.Add(new Setter(WindowStyleProperty, WindowStyle.None));
                style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0d)));

                double desiredWidth = 475d;

                double top = SystemParameters.WorkArea.Top + 50d;
                double left = SystemParameters.WorkArea.Right - desiredWidth - 100d;

                style.Setters.Add(new Setter(TopProperty, top));
                style.Setters.Add(new Setter(LeftProperty, left));

                style.Setters.Add(new Setter(SizeToContentProperty, SizeToContent.Height));
                style.Setters.Add(new Setter(WidthProperty, desiredWidth));

                return style;
            }

            private static Grid BuildGrid(int numRows)
            {
                if (numRows < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(numRows));
                }

                Grid grid = new Grid
                {
                    Style = BuildGridStyle()
                };

                for (int i = 0; i < numRows; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });
                }

                return grid;
            }

            private static Style BuildGridStyle()
            {
                Style style = new Style(typeof(Grid));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));

                style.Setters.Add(new Setter(HeightProperty, Double.NaN));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Stretch));

                style.Setters.Add(new Setter(WidthProperty, Double.NaN));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

                return style;
            }

            private static Label BuildLabel(Style labelStyle, string text, FontStyle fontStyle)
            {
                return new Label
                {
                    Style = labelStyle,
                    Content = new TextBlock
                    {
                        Text = text,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        FontStyle = fontStyle
                    }
                };
            }

            private static Style BuildTitleLabelStyle()
            {
                Style style = new Style(typeof(Label));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(MarginProperty, new Thickness(15d, 0d, 15d, 0d)));
                style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Calibri")));
                style.Setters.Add(new Setter(FontSizeProperty, 22d));
                style.Setters.Add(new Setter(HeightProperty, 75d));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Center));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

                return style;
            }

            private static Style BuildDescriptionLabelStyle()
            {
                Style style = new Style(typeof(Label));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(MarginProperty, new Thickness(0d, 0d, 15d, 0d)));
                style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Calibri")));
                style.Setters.Add(new Setter(FontSizeProperty, 14d));
                style.Setters.Add(new Setter(HeightProperty, 40d));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Top));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Right));

                return style;
            }
        }
    }
}
