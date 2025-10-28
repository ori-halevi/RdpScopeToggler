using Prism.Mvvm;
using RdpScopeToggler.Services.ServiceInstallationManager;

namespace RdpScopeToggler.ViewModels
{
    public class WaitingForServiceUserControlViewModel : BindableBase
    {
        private string text;
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        public WaitingForServiceUserControlViewModel(IServiceInstallationManager serviceInstallationManager)
        {
            Text = "Waiting for service...";
            serviceInstallationManager.StepStarted += (args) => Text = args;
        }
    }
}
