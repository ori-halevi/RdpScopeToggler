using Prism.Mvvm;

namespace RdpScopeToggler.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "RDP Scope Toggler";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}
