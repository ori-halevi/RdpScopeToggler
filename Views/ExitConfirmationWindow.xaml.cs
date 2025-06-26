using System.Windows;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for ExitConfirmationWindow.xaml
    /// </summary>
    public partial class ExitConfirmationWindow : Window
    {
        public bool UserConfirmed { get; private set; } = false;

        public ExitConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = false;
            Close();
        }
    }
}
