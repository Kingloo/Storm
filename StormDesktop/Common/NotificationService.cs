using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StormDesktop.Common
{
	public static class NotificationService
	{
		private static bool canShowNotification = true;

		private readonly static Queue<Notification> notificationQueue = new Queue<Notification>();

		/// <summary>
		/// How many times the timer ticked but found nothing in the queue to work on.
		/// </summary>
		private static int timerTickCount = 0;

		/// <summary>
		/// After this many ticks of the timer where it found nothing to work on, we turn off the timer until a new notification is sent.
		/// </summary>
		private const int timerTickMax = 15;

		private static DispatcherTimer? queuePullTimer = null;



		public static void Send(string title) => Send(title, string.Empty, () => { });

		public static void Send(string title, string description) => Send(title, description, () => { });

		public static void Send(string title, Action action) => Send(title, string.Empty, action);

		public static void Send(string title, string description, Action action)
		{
			InitTimer();

			Notification notification = new Notification(title, description, action);

			notification.Closed += (s, e) => canShowNotification = true;

			notificationQueue.Enqueue(notification);
		}



		private static void InitTimer()
		{
			if (queuePullTimer is null)
			{
				queuePullTimer = new DispatcherTimer(DispatcherPriority.Background)
				{
					Interval = TimeSpan.FromSeconds(1.5d)
				};

				queuePullTimer.Tick += QueuePullTimer_Tick;
			}

			if (!queuePullTimer.IsEnabled)
			{
				queuePullTimer.Start();
			}
		}

		private static void QueuePullTimer_Tick(object? sender, EventArgs e)
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
				queuePullTimer?.Stop();

				timerTickCount = 0; // and reset the counter
			}
		}

		private static void Display(Notification notification)
		{
			notification.Show();

			System.Media.SystemSounds.Hand.Play();
		}



		private sealed class Notification : Window
		{
			private readonly DispatcherCountdownTimer closeTimer;

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
				double interval = 4d;
#else
                double interval = 15d;
#endif

				closeTimer = new DispatcherCountdownTimer(TimeSpan.FromSeconds(interval), Close);

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

#if DEBUG
				style.Setters.Add(new Setter(BackgroundProperty, Brushes.DarkGoldenrod));
#else
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
#endif
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
					grid.RowDefinitions.Add(
						new RowDefinition
						{
							Height = GridLength.Auto
						});
				}

				return grid;
			}

			[System.Diagnostics.DebuggerStepThrough]
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

			[System.Diagnostics.DebuggerStepThrough]
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

			[System.Diagnostics.DebuggerStepThrough]
			private static Style BuildTitleLabelStyle()
			{
				Style style = new Style(typeof(Label));

				style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
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

			[System.Diagnostics.DebuggerStepThrough]
			private static Style BuildDescriptionLabelStyle()
			{
				Style style = new Style(typeof(Label));

				style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
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
