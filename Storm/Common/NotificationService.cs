using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Storm.Common
{
    public static class NotificationService
    {
        public static void Send(string title)
            => Send(title, string.Empty, null);

        public static void Send(string title, string description)
            => Send(title, description, null);

        public static void Send(string title, Action action)
            => Send(title, string.Empty, action);
        
        public static void Send(string title, string description, Action action)
        {
            var window = new NotificationWindow(title, description, action);

            Display(window);
        }


        private static void Display(NotificationWindow window)
        {
            if (window == null) { throw new ArgumentNullException(nameof(window)); }

            window.Show();

            System.Media.SystemSounds.Hand.Play();
        }


        private sealed class NotificationWindow : Window
        {
            internal NotificationWindow(string title, string description, Action action)
            {
                Style = BuildWindowStyle(action);
                
                bool hasDescription = !String.IsNullOrWhiteSpace(description);

                Grid grid = hasDescription ? BuildGrid(numRows: 2) : BuildGrid(numRows: 1);

                Label lbl_Title = BuildTitleLabel(title);
                Grid.SetRow(lbl_Title, 0);
                grid.Children.Add(lbl_Title);

                if (hasDescription)
                {
                    Label lbl_Description = BuildDescriptionLabel(description);
                    Grid.SetRow(lbl_Description, 1);
                    grid.Children.Add(lbl_Description);
                }
                
                AddChild(grid);
                
#if DEBUG
                var notifyWindowCloseTimer = new DispatcherCountdownTimer(
                    TimeSpan.FromSeconds(2),
                    () => Close());
#else
                var notifyWindowCloseTimer = new DispatcherCountdownTimer(
                    TimeSpan.FromSeconds(15),
                    () => Close());
#endif
                notifyWindowCloseTimer.Start();
            }

            private static Style BuildWindowStyle(Action action)
            {
                Style style = new Style(typeof(NotificationWindow));

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

                double top = SystemParameters.WorkArea.Top + 50;
                double left = SystemParameters.WorkArea.Right - desiredWidth - 100;

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
                    throw new ArgumentException("there must be at least 1 row", nameof(numRows));
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

            private static Label BuildTitleLabel(string title)
            {
                return new Label
                {
                    Style = BuildTitleLabelStyle(),
                    Content = new TextBlock
                    {
                        Text = title,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                };
            }

            private static Style BuildTitleLabelStyle()
            {
                Style style = new Style(typeof(Label));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(MarginProperty, new Thickness(15, 0, 15, 0)));
                style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Calibri")));
                style.Setters.Add(new Setter(FontSizeProperty, 22d));
                style.Setters.Add(new Setter(HeightProperty, 75d));
                style.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Stretch));
                style.Setters.Add(new Setter(VerticalContentAlignmentProperty, VerticalAlignment.Center));
                style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

                return style;
            }

            private static Label BuildDescriptionLabel(string description)
            {
                return new Label
                {
                    Style = BuildDescriptionLabelStyle(),
                    Content = new TextBlock
                    {
                        Text = description,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        FontStyle = FontStyles.Italic
                    }
                };
            }

            private static Style BuildDescriptionLabelStyle()
            {
                Style style = new Style(typeof(Label));

                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                style.Setters.Add(new Setter(ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 0, 15, 0)));
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
