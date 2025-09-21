using Prism.Mvvm;
using System.Reflection;

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

        private string version = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
        public string Version
        {
            get { return version; }
            set { SetProperty(ref version, value); }
        }
    }
}
