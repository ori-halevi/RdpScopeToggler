using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace RdpScopeToggler.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;
        public bool CollapseInsteadOfHide { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;

            if (Invert)
                boolValue = !boolValue;

            if (boolValue)
                return Visibility.Visible;

            return CollapseInsteadOfHide ? Visibility.Collapsed : Visibility.Hidden;
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
