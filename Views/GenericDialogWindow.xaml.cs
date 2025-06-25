using RdpScopeToggler.Models;
using System.Windows;
using System.Windows.Controls;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for GenericDialogWindow.xaml
    /// </summary>
    public partial class GenericDialogWindow : Window
    {
        private readonly GenericDialogOptions _options;
        private bool _buttonClicked = false;

        public GenericDialogWindow(GenericDialogOptions options)
        {
            InitializeComponent();
            _options = options;

            this.Title = options.Title;
            this.Topmost = options.Topmost;
            MessageTextBlock.Text = options.Message;

            if (options.Icon != null)
            {
                DialogIcon.Source = options.Icon;
                DialogIcon.Visibility = Visibility.Visible;
            }

            foreach (var btnConfig in options.Buttons)
            {
                var button = new Button
                {
                    Content = btnConfig.Text,
                    Margin = new Thickness(10, 0, 0, 0),
                    IsDefault = btnConfig.IsDefault,
                    IsCancel = btnConfig.IsCancel,
                    MinWidth = 80
                };

                button.Click += (s, e) =>
                {
                    _buttonClicked = true;
                    btnConfig.OnClick?.Invoke();
                    this.Close();
                };

                ButtonsPanel.Children.Add(button);
            }

            this.Closing += (sender, e) =>
            {
                if (!_buttonClicked)
                {
                    // משתמש סגר עם X או בכל דרך אחרת
                    // נפעיל OnClose רק אם קיים
                    _options.OnClose?.Invoke();

                    // אם רוצים למנוע סגירה אפשר לשים: e.Cancel = true;
                    // אבל פה נניח שהחלון כן ייסגר
                }
            };
        }
    }

}
