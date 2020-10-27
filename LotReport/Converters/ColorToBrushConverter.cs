using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LotReport.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(Color))
            {
                throw new InvalidOperationException($"Unsupported type [{value.GetType().Name}], {nameof(ColorToBrushConverter)}.Convert()");
            }

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
