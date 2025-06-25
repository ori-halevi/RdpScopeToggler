using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for AnyClockUserControl
    /// </summary>
    public partial class AnyClockUserControl : UserControl
    {
        public AnyClockUserControl()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateSecondsVisibility();
        }
        // --- TimeTitle ---
        public static readonly DependencyProperty TimeTitleProperty =
            DependencyProperty.Register(
                nameof(TimeTitle),
                typeof(string),
                typeof(AnyClockUserControl),
                new FrameworkPropertyMetadata("Temp"));

        public string TimeTitle
        {
            get => (string)GetValue(TimeTitleProperty);
            set => SetValue(TimeTitleProperty, value);
        }

        // --- TimeTitleVisibility ---
        public static readonly DependencyProperty TimeTitleVisibilityProperty =
            DependencyProperty.Register(
                nameof(TimeTitleVisibility),
                typeof(Visibility),
                typeof(AnyClockUserControl),
                new PropertyMetadata(Visibility.Collapsed));

        public Visibility TimeTitleVisibility
        {
            get => (Visibility)GetValue(TimeTitleVisibilityProperty);
            set => SetValue(TimeTitleVisibilityProperty, value);
        }


        // --- DAYS (0..365) ---
        public static readonly DependencyProperty CountDownDayProperty =
            DependencyProperty.Register(
                nameof(CountDownDay),
                typeof(int),
                typeof(AnyClockUserControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    CoerceDay),
                ValidateDay);

        public int CountDownDay
        {
            get => (int)GetValue(CountDownDayProperty);
            set => SetValue(CountDownDayProperty, value);
        }

        private static bool ValidateDay(object value)
        {
            var v = (int)value;
            return v >= 0 && v <= 365;
        }

        private static object CoerceDay(DependencyObject d, object baseValue)
        {
            var v = (int)baseValue;
            return v < 0 ? 0 : (v > 365 ? 365 : v);
        }

        // --- HOURS (0..23) ---
        public static readonly DependencyProperty CountDownHourProperty =
            DependencyProperty.Register(
                nameof(CountDownHour),
                typeof(int),
                typeof(AnyClockUserControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    CoerceHour),
                ValidateHour);

        public int CountDownHour
        {
            get => (int)GetValue(CountDownHourProperty);
            set => SetValue(CountDownHourProperty, value);
        }

        private static bool ValidateHour(object value)
        {
            var v = (int)value;
            return v >= 0 && v <= 23;
        }

        private static object CoerceHour(DependencyObject d, object baseValue)
        {
            var v = (int)baseValue;
            return v < 0 ? 0 : (v > 23 ? 23 : v);
        }

        // --- MINUTES (0..59) ---
        public static readonly DependencyProperty CountDownMinuteProperty =
            DependencyProperty.Register(
                nameof(CountDownMinute),
                typeof(int),
                typeof(AnyClockUserControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    CoerceMinute),
                ValidateMinute);

        public int CountDownMinute
        {
            get => (int)GetValue(CountDownMinuteProperty);
            set => SetValue(CountDownMinuteProperty, value);
        }

        private static bool ValidateMinute(object value)
        {
            var v = (int)value;
            return v >= 0 && v <= 59;
        }

        private static object CoerceMinute(DependencyObject d, object baseValue)
        {
            var v = (int)baseValue;
            return v < 0 ? 0 : (v > 59 ? 59 : v);
        }

        // --- SECONDS (0..59) ---
        public static readonly DependencyProperty CountDownSecondProperty =
            DependencyProperty.Register(
                nameof(CountDownSecond),
                typeof(int),
                typeof(AnyClockUserControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    CoerceSecond),
                ValidateSecond);

        public int CountDownSecond
        {
            get => (int)GetValue(CountDownSecondProperty);
            set => SetValue(CountDownSecondProperty, value);
        }

        private static bool ValidateSecond(object value)
        {
            var v = (int)value;
            return v >= 0 && v <= 59;
        }

        private static object CoerceSecond(DependencyObject d, object baseValue)
        {
            var v = (int)baseValue;
            return v < 0 ? 0 : (v > 59 ? 59 : v);
        }

        #region Show seconds
        public static readonly DependencyProperty ShowSecondsProperty =
            DependencyProperty.Register(
                nameof(ShowSeconds),
                typeof(bool),
                typeof(AnyClockUserControl),
                new PropertyMetadata(true, OnShowSecondsChanged));

                public bool ShowSeconds
                {
                    get => (bool)GetValue(ShowSecondsProperty);
                    set => SetValue(ShowSecondsProperty, value);
                }

                private static void OnShowSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
                {
                    if (d is AnyClockUserControl control)
                    {
                        control.UpdateSecondsVisibility();
                    }
                }

                private void UpdateSecondsVisibility()
                {
                    var visibility = ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
                    IntUpDownSecond.Visibility = visibility;
                    TextBlockSecond.Visibility = visibility;
                    TextBlockColonBeforeSeconds.Visibility = visibility;
                    BorderSeconds.Visibility = visibility;

            // הנקודתיים שלפני שניות (Grid.Column="5")
            foreach (var child in ((Grid)this.Content).Children)
                    {
                        if (child is TextBlock tb && Grid.GetColumn(tb) == 5 && Grid.GetRow(tb) == 1)
                        {
                            tb.Visibility = visibility;
                        }
                    }
                }

        #endregion

        // --- ReadOnly Toggle ---
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(AnyClockUserControl),
                new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        // --- MouseWheel handler (עם בדיקת IsReadOnly) ---
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is Xceed.Wpf.Toolkit.IntegerUpDown upDown && !IsReadOnly)
            {
                int inc = upDown.Increment ?? 1;
                if (e.Delta > 0) upDown.Value += inc;
                else upDown.Value -= inc;

                e.Handled = true;
            }
        }

        private void OnIntegerUpDownLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Xceed.Wpf.Toolkit.IntegerUpDown upDown)
            {
                // גורם לו לקרוא ל־CoerceValue
                var dp = Xceed.Wpf.Toolkit.IntegerUpDown.ValueProperty;
                var value = upDown.GetValue(dp);
                upDown.SetValue(dp, value);
            }
        }

        // Triggered when the value of the control changes (e.g., user input)
        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is Xceed.Wpf.Toolkit.IntegerUpDown upDown && upDown.Value is int val)
            {
                int min = (int)(upDown.Minimum ?? int.MinValue);
                int max = (int)(upDown.Maximum ?? int.MaxValue);
                if (val < min)
                    upDown.Value = min;
                else if (val > max)
                    upDown.Value = max;
            }
        }




        

        private TextBox? GetInnerTextBox(Xceed.Wpf.Toolkit.IntegerUpDown upDown)
            {
                return FindVisualChild<TextBox>(upDown);
            }

            private T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child is T t)
                        return t;

                    T? childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
                return null;
            }

    private void IntegerUpDown_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is Xceed.Wpf.Toolkit.IntegerUpDown upDown)
            {
                var textBox = GetInnerTextBox(upDown);
                if (textBox == null)
                    return;

                string currentText = textBox.Text;
                int caretIndex = textBox.CaretIndex;

                // סימון אם יש טקסט מסומן (החלפה)
                string proposedText;
                if (textBox.SelectionLength > 0)
                {
                    proposedText = currentText.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                              .Insert(textBox.SelectionStart, e.Text);
                }
                else
                {
                    proposedText = currentText.Insert(caretIndex, e.Text);
                }

                if (!int.TryParse(proposedText, out int result))
                {
                    e.Handled = true;
                    return;
                }

                int min = (int)(upDown.Minimum ?? int.MinValue);
                int max = (int)(upDown.Maximum ?? int.MaxValue);
                if (result < min || result > max)
                {
                    e.Handled = true;
                }
            }
        }

        private void IntegerUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true; // חסום Ctrl+V
            }
        }


    }
}
