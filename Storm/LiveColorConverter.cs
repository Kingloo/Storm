using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Storm
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    class LiveColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool islive = (bool)value;
            SolidColorBrush brush = null;

            switch (islive)
            {
                case true:
                    brush = (SolidColorBrush)Application.Current.Resources["Online"];
                    break;
                case false:
                    brush = (SolidColorBrush)Application.Current.Resources["Offline"];
                    break;
                default:
                    brush = (SolidColorBrush)Application.Current.Resources["Default"];
                    break;
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
