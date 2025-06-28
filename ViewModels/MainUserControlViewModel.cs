using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace RdpScopeToggler.ViewModels
{
    public class MainUserControlViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public enum Tabs
        {
            Home,
            WhiteList,
            LocalAddresses
        }

        private Tabs _selectedTab;
        public Tabs SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                    NavigateToTab(value);
            }
        }

        public MainUserControlViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SelectedTab = Tabs.Home; // ברירת מחדל
        }

        private void NavigateToTab(Tabs tab)
        {
            switch (tab)
            {
                case Tabs.Home:
                    _regionManager.RequestNavigate("CardsRegion", "HomeUserControl");
                    break;
                case Tabs.WhiteList:
                    _regionManager.RequestNavigate("CardsRegion", "WhiteListUserControl");
                    break;
                case Tabs.LocalAddresses:
                    _regionManager.RequestNavigate("CardsRegion", "LocalAddressesUserControl");
                    break;
            }
        }
    }

}
