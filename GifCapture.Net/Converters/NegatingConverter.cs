using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GifCapture.Converters
{
    public class NegatingConverter : IValueConverter
    {
        static object DoConvert(object value)
        {
            if (value is bool b)
                return !b;

            return Binding.DoNothing;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case bool b when targetType == typeof(Visibility):
                    return b ? Visibility.Collapsed : Visibility.Visible;

                case bool b:
                    return !b;

                case Visibility visibility when targetType == typeof(Visibility):
                    return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

                case Visibility visibility:
                    return visibility == Visibility.Collapsed;

                default:
                    return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DoConvert(value);
        }
    }
}