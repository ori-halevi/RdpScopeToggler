using System;
using System.Globalization;
using System.Windows.Data;

namespace RdpScopeToggler.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string paramString && Enum.IsDefined(value.GetType(), value))
            {
                var enumValue = Enum.Parse(value.GetType(), paramString);
                return enumValue.Equals(value);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string paramString && bool.TryParse(value?.ToString(), out bool useValue) && useValue)
            {
                return Enum.Parse(targetType, paramString);
            }
            return Binding.DoNothing;
        }
    }

}
