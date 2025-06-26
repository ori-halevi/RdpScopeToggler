using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RdpScopeToggler.ViewModels
{
    public class SettingsUserControlViewModel : BindableBase, INavigationAware
    {
        public ObservableCollection<string> LanguagesOptions { get; }

        private string selectedLanguage;
        public string SelectedLanguage
        {
            get { return selectedLanguage; }
            set { SetProperty(ref selectedLanguage, value); }
        }
        public ICommand CloseCommand { get; }
        public SettingsUserControlViewModel(IRegionManager regionManager)
        {
            LanguagesOptions = new ObservableCollection<string>
            {
                "עברית",
                "English"
            };
            SelectedLanguage = LanguagesOptions[0];

            CloseCommand = new DelegateCommand(() =>
            {
                regionManager.RequestNavigate("ContentRegion", "HomeUserControl");
            });
        }

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
