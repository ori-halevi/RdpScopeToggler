using Prism.Mvvm;

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

        public WaitingForServiceUserControlViewModel()
        {
            Text = "Waiting for service...";
        }
    }
}
