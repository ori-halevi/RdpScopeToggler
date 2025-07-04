using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Managers;
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
            get => selectedLanguage;
            set
            {
                if (SetProperty(ref selectedLanguage, value))
                {
                    // Call language manager
                    if (value == "עברית")
                        LanguageManager.ChangeLanguage("he");
                    else
                        LanguageManager.ChangeLanguage("en");
                }
            }
        }

        public ICommand CloseCommand { get; }

        private readonly IRegionManager regionManager;

        public SettingsUserControlViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            LanguagesOptions = new ObservableCollection<string>
            {
                "English",
                "עברית"
            };

            SelectedLanguage = LanguagesOptions[0]; // default

            CloseCommand = new DelegateCommand(() =>
            {
                regionManager.RequestNavigate("ContentRegion", "MainUserControl");
            });
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // If needed, handle navigation parameters
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
