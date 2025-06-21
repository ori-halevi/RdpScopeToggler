using System.Windows.Controls;
using System.Windows.Input;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for HomeUserControl
    /// </summary>
    public partial class HomeUserControl : UserControl
    {
        public HomeUserControl()
        {
            InitializeComponent();
        }
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is Xceed.Wpf.Toolkit.IntegerUpDown upDown)
            {
                // שים לב ל־Increment והכיוון
                int increment = upDown.Increment ?? 1;
                if (e.Delta > 0)
                    upDown.Value += increment;
                else
                    upDown.Value -= increment;

                // עצור את האירוע שלא ימשיך הלאה
                e.Handled = true;
            }
        }

    }
}
