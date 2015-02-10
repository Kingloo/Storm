using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Storm
{
    public static class NotificationService
    {
        public static void Send(string send)
        {
            Notification n = new Notification(send);
            n.Send();
        }

        public static void Send(string send, string description)
        {
            Notification n = new Notification(send, description);
            n.Send();
        }

        public static void Send(string send, Uri uri)
        {
            Notification n = new Notification(send, uri);
            n.Send();
        }

        public static void Send(string send, string description, Uri uri)
        {
            Notification n = new Notification(send, description, uri);
            n.Send();
        }

        private class Notification
        {
            private string _title = string.Empty;
            private string _description = string.Empty;
            private Uri _uri = null;

            public string Title { get { return this._title; } }
            public string Description { get { return this._description; } }
            public Uri Uri { get { return this._uri; } }

            public Notification(string title)
            {
                this._title = title;
            }

            public Notification(string title, string description)
            {
                this._title = title;
                this._description = description;
            }

            public Notification(string title, Uri doubleClickUri)
            {
                this._title = title;
                this._uri = doubleClickUri;
            }

            public Notification(string title, string description, Uri doubleClickUri)
            {
                this._title = title;
                this._description = description;
                this._uri = doubleClickUri;
            }

            public void Send()
            {
                NotificationWindow notificationWindow = new NotificationWindow(this);
            }
        }

        private class NotificationWindow : Window
        {
            private DispatcherTimer _expirationTimer = null;
            private Notification _n = null;

            public NotificationWindow(Notification n)
            {
                this._n = n;

                this.Owner = Application.Current.MainWindow;

                this.Style = BuildWindowStyle();

                BuildTimer();

                Grid grid = new Grid { Style = BuildGridStyle() };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Label labelTitle = new Label
                {
                    Content = new TextBlock
                    {
                        Text = this._n.Title,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    },
                    Style = BuildLabelTitleStyle()
                };

                Grid.SetRow(labelTitle, 0);

                grid.Children.Add(labelTitle);

                if (!(String.IsNullOrEmpty(n.Description)))
                {
                    Label labelDescription = new Label
                    {
                        Content = new TextBlock
                        {
                            Text = this._n.Description,
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            FontStyle = FontStyles.Italic
                        },
                        Style = BuildLabelDescriptionStyle()
                    };

                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    Grid.SetRow(labelDescription, 1);

                    grid.Children.Add(labelDescription);
                }

                this.AddChild(grid);

                DisplayThisWindow();
            }

            private void BuildTimer()
            {
                this._expirationTimer = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 15)
                };

                this._expirationTimer.Tick += Expiration_Tick;
                this._expirationTimer.IsEnabled = true;
            }

            private void Expiration_Tick(object sender, EventArgs e)
            {
                this._expirationTimer.Tick -= Expiration_Tick;
                this._expirationTimer.IsEnabled = false;
                this._expirationTimer = null;
                
                this.Close();
            }

            private Style BuildWindowStyle()
            {
                Style style = new Style(typeof(NotificationWindow));

                style.Setters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler((sender, e) =>
                {
                    // we deliberately avoid using Utils.OpenUriInBrowser to avoid the dependency

                    Process.Start(this._n.Uri.AbsoluteUri);
                })));

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
                style.Setters.Add(new Setter(MarginProperty, new Thickness(15, 0, 0, 0)));
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
