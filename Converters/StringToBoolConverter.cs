using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RdpScopeToggler.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        // Convert string to bool for IsChecked
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value?.ToString() == parameter?.ToString());
        }

        // Convert back from bool to string for SelectedAction
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return parameter?.ToString();
            return Binding.DoNothing;
        }
    }
}
