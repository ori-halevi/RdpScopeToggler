using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RdpScopeToggler.Converters
{
    public class BoolToCollapsedConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;

            if (Invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                if (Invert)
                    return visibility != Visibility.Visible;

                return visibility == Visibility.Visible;
            }

            return false;
        }
    }
}
