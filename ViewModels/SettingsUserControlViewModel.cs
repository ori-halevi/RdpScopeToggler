using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeToggler.Enums;
using RdpScopeToggler.Services.LanguageService;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.ServiceInstallationManager;
using RdpScopeToggler.Services.SettingsService;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace RdpScopeToggler.ViewModels
{
    public class SettingsUserControlViewModel : BindableBase, INavigationAware
    {
        public ObservableCollection<string> LanguagesOptions { get; }
        public ObservableCollection<ActionsEnum> StateOptions { get; }

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

        private ActionsEnum selectedState;
        public ActionsEnum SelectedState
        {
            get => selectedState;
            set
            {
                if (SetProperty(ref selectedState, value))
                {
                    settingsService.SetState(value);
                }
            }
        }

        private bool isRefreshingService;
        public bool IsRefreshingService
        {
            get => isRefreshingService;
            set => SetProperty(ref isRefreshingService, value);
        }

        public ICommand CloseCommand { get; }
        public ICommand OpenLogsFolderCommand { get; }
        public ICommand RefreshRdpScopeServiceCommand { get; }

        private readonly IRegionManager regionManager;
        private readonly IServiceInstallationManager serviceInstallationManager;
        private readonly ILanguageService languageService;
        private readonly ISettingsService settingsService;
        private readonly ILoggerService loggerService;

        public SettingsUserControlViewModel(IRegionManager regionManager, ILanguageService languageService, ISettingsService settingsService, IServiceInstallationManager serviceInstallationManager, ILoggerService loggerService)
        {
            this.regionManager = regionManager;
            this.serviceInstallationManager = serviceInstallationManager;
            this.settingsService = settingsService;
            this.languageService = languageService;
            this.loggerService = loggerService;

            LanguagesOptions = new ObservableCollection<string>
            {
                "English",
                "עברית"
            };

            StateOptions = new ObservableCollection<ActionsEnum>
            {
                ActionsEnum.RemoteSystems,
                ActionsEnum.WhiteList,
                ActionsEnum.LocalComputersAndWhiteList,
                ActionsEnum.LocalComputers,
                ActionsEnum.CloseRdp
            };

            var l = languageService.SelectedLanguage;
            if (l == "עברית" || l == "he")
            {
                SelectedLanguage = "עברית";
            }
            else
            {
                SelectedLanguage = "English";
            }

            SelectedState = settingsService.GetState();

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

            // async lambda -> async void at runtime: any uncaught exception would crash
            // the app via the sync context, so wrap even the logger call defensively.
            RefreshRdpScopeServiceCommand = new DelegateCommand(
                async () =>
                {
                    IsRefreshingService = true;
                    try
                    {
                        await serviceInstallationManager.RefreshServiceAsync();
                    }
                    catch (Exception ex)
                    {
                        try { loggerService.Error("RefreshRdpScopeServiceCommand failed.", ex); }
                        catch { /* swallow: logger must never crash an async-void command */ }
                    }
                    finally
                    {
                        IsRefreshingService = false;
                    }
                },
                () => !IsRefreshingService)
                .ObservesProperty(() => IsRefreshingService);

        }

        #region Navigation Methods

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        #endregion
    }
}
