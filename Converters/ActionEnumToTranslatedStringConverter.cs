using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace RdpScopeToggler.Converters
{
    public class ActionEnumToTranslatedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                string key = enumValue.ToString() + "_translator";
                var translated = Application.Current.TryFindResource(key) as string;
                return translated ?? enumValue.ToString();
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // מנסה להתאים טקסט חזרה ל-enum לפי מפתח התרגום
            foreach (var field in Enum.GetValues(typeof(RdpScopeToggler.Enums.ActionsEnum)))
            {
                string key = field + "_translator";
                var translated = Application.Current.TryFindResource(key) as string;
                if (translated == (string)value)
                    return field;
            }

            // fallback: אם לא נמצא תרגום תואם
            return Enum.Parse(typeof(RdpScopeToggler.Enums.ActionsEnum), value.ToString());
        }
    }
}
