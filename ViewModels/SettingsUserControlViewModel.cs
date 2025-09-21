using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Services.LanguageService;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
                    if (value == "עברית" || value == "he")
                    {
                        languageService.SetLanguage("he");
                    }
                    else
                    {
                        languageService.SetLanguage("en");
                    }
                }
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand OpenLogsFolderCommand { get; }

        private readonly IRegionManager regionManager;
        private readonly ILanguageService languageService;

        public SettingsUserControlViewModel(IRegionManager regionManager, ILanguageService languageService)
        {
            this.regionManager = regionManager;
            this.languageService = languageService;

            LanguagesOptions = new ObservableCollection<string>
            {
                "English",
                "עברית"
            };

            SelectedLanguage = languageService.SelectedLanguage;

            CloseCommand = new DelegateCommand(() =>
            {
                regionManager.RequestNavigate("ContentRegion", "MainUserControl");
            });

            OpenLogsFolderCommand = new DelegateCommand(() =>
            {
                string pathToLoggerFolder = "C:\\ProgramData\\RdpScopeToggler\\Logs";

                if (Directory.Exists(pathToLoggerFolder))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = pathToLoggerFolder,
                        UseShellExecute = true
                    });
                }
                else
                {
                    throw new DirectoryNotFoundException("Logger folder was not found.");
                }
            });

        }

        #region Navigation Methods

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
