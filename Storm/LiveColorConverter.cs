using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Storm
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    class LiveColorConverter : IValueConverter
    {
        public SolidColorBrush Online { get; set; }
        public SolidColorBrush Offline { get; set; }
        public SolidColorBrush Default { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool islive = (bool)value;
            SolidColorBrush brush = null;

            switch (islive)
            {
                case true:
                    brush = this.Online;
                    break;
                case false:
                    brush = this.Offline;
                    break;
                default:
                    brush = this.Default;
                    break;
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
