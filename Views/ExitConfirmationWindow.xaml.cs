using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
