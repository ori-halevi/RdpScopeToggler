using RdpScopeToggler.ViewModels;
using System.Windows.Controls;
using Prism.Ioc;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for IndicatorsUserControl
    /// </summary>
    public partial class IndicatorsUserControl : UserControl
    {
        public IndicatorsUserControl()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Container.Resolve<IndicatorsUserControlViewModel>();
        }
    }
}
