using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Storm.Common
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class LiveColorConverter : IValueConverter
    {
        public SolidColorBrush Online { get; set; }
        public SolidColorBrush Offline { get; set; }
        public SolidColorBrush Default { get; set; }
        
        public LiveColorConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? Online : Offline;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => true;
    }
}
