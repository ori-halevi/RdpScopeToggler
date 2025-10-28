using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RdpScopeToggler.Converters
{
    public class IndicatorBrushConverter : IValueConverter
    {
        // Convert returns either a Brush (for Fill/Border/Foreground) or a double (for DropShadowEffect.Opacity)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isActive = value is bool b && b;

            // parameter examples: "Purple|Outer" or "Purple|Inner" or just "Purple"
            string param = parameter?.ToString() ?? "Gray|Outer";
            string[] parts = param.Split('|');
            string colorName = parts[0];
            string layer = parts.Length > 1 ? parts[1] : "Outer";

            // try find brushes in resources, otherwise fallback to simple brushes
            Brush lightGrayBrush = Application.Current.TryFindResource("LightGrayBrush") as Brush
                                   ?? new SolidColorBrush(Colors.LightGray);
            Brush mediumGrayBrush = Application.Current.TryFindResource("MediumGrayBrush") as Brush
                                    ?? new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));

            // If target is a numeric type, return shadow opacity
            if (targetType == typeof(double) || targetType == typeof(float) || targetType == typeof(decimal))
            {
                return isActive ? 0.5 : 0.0;
            }

            // For brushes: when inactive return appropriate gray depending on layer
            if (!isActive)
            {
                return layer.Equals("Inner", StringComparison.OrdinalIgnoreCase)
                    ? mediumGrayBrush
                    : lightGrayBrush;
            }

            // Active -> try return color brush (e.g. "PurpleBrush")
            string activeBrushKey = colorName + "Brush";
            var activeBrush = Application.Current.TryFindResource(activeBrushKey) as Brush;
            return activeBrush ?? lightGrayBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
