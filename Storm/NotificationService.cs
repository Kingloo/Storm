using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Storm
{
    public static class NotificationService
    {
        public static void Send(string title, Action action)
        {
            NotificationWindow window = new NotificationWindow(title, string.Empty, action);
        }

        public static void Send(string title, string description, Action action)
        {
            NotificationWindow window = new NotificationWindow(title, description, action);
        }

        private class NotificationWindow : Window
        {
            private Action action = null;

            internal NotificationWindow(string title, string description, Action action)
            {
                this.action = action;

                this.Style = BuildWindowStyle();

                Grid grid = new Grid
                {
                    Style = BuildGridStyle()
                };

                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });

                Label lbl_Title = new Label
                {
                    Style = BuildLabelTitleStyle(),
                    Content = new TextBlock
                    {
                        Text = title,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                };

                Grid.SetRow(lbl_Title, 0);
                grid.Children.Add(lbl_Title);

                if (String.IsNullOrEmpty(description) == false)
                {
                    Label lbl_Description = new Label
                    {
                        Style = BuildLabelDescriptionStyle(),
                        Content = new TextBlock
                        {
                            Text = description,
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            FontStyle = FontStyles.Italic
                        }
                    };

                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });

                    Grid.SetRow(lbl_Description, 1);
                    grid.Children.Add(lbl_Description);
                }

                this.AddChild(grid);

#if DEBUG
                CountdownDispatcherTimer expirationTimer = new CountdownDispatcherTimer(new TimeSpan(0, 0, 2), () => this.Close());
#else
                CountdownDispatcherTimer expirationTimer = new CountdownDispatcherTimer(new TimeSpan(0, 0, 15), () => this.Close());
#endif

                DisplayThisWindow();
            }

            private Style BuildWindowStyle()
            {
                Style style = new Style(typeof(NotificationWindow));

                if (action != null)
                {
                    MouseButtonEventHandler doubleClickAction = (sender, e) => action();

                    EventSetter leftMouseDoubleClick = new EventSetter(MouseDoubleClickEvent, doubleClickAction);

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

                double top = SystemParameters.WorkArea.Top + 50;
                double left = SystemParameters.WorkArea.Right - 475d - 100;

                style.Setters.Add(new Setter(TopProperty, top));
                style.Setters.Add(new Setter(LeftProperty, left));

                style.Setters.Add(new Setter(SizeToContentProperty, SizeToContent.Height));
                style.Setters.Add(new Setter(WidthProperty, 475d));

                return style;
            }

            private Style BuildGridStyle()
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

            private Style BuildLabelTitleStyle()
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

            private Style BuildLabelDescriptionStyle()
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

            private void DisplayThisWindow()
            {
                this.Show();

                System.Media.SystemSounds.Hand.Play();
            }
        }
    }
}
