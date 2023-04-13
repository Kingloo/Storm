using System;
using System.Globalization;
using System.Windows.Data;

namespace StormDesktop.Converters
{
	[ValueConversion(typeof(object), typeof(bool))]
	public class IsNotNullConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is not null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException($"{nameof(IsNotNullConverter)} does not support converting back");
		}
	}
}
