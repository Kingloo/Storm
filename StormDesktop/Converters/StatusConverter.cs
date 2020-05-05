using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using StormLib;

namespace StormDesktop.Converters
{
    [ValueConversion(typeof(Status), typeof(Brush))]
    public class StatusConverter : DependencyObject, IValueConverter
    {
        public Brush None { get; set; } = Brushes.Transparent;
        public Brush Unsupported { get; set; } = Brushes.Transparent;
        public Brush Public { get; set; } = Brushes.Transparent;
        public Brush Private { get; set; } = Brushes.Transparent;
        public Brush Banned { get; set; } = Brushes.Transparent;
        public Brush Rerun { get; set; } = Brushes.Transparent;
        public Brush Offline { get; set; } = Brushes.Transparent;
        public Brush Unknown { get; set; } = Brushes.Transparent;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Status status)
            {
                return status switch
                {
                    Status.None => None,
                    Status.Unsupported => Unsupported,
                    Status.Public => Public,
                    Status.Private => Private,
                    Status.Banned => Banned,
                    Status.Rerun => Rerun,
                    Status.Offline => Offline,
                    Status.Unknown => Unknown,
                    _ => throw new ArgumentException("you submitted a Status enum value that does not exist", nameof(value))
                };
            }
            else
            {
                return None;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
