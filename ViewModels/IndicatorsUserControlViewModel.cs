using RdpScopeToggler.Services.RdpService;
using Prism.Mvvm;
using System.Diagnostics;

namespace RdpScopeToggler.ViewModels
{
    public class IndicatorsUserControlViewModel : BindableBase
    {
        private bool isInternalOpen = true;
        public bool IsInternalOpen
        {
            get => isInternalOpen;
            set => SetProperty(ref isInternalOpen, value);
        }

        private bool isExternalOpen;
        public bool IsExternalOpen
        {
            get => isExternalOpen;
            set => SetProperty(ref isExternalOpen, value);
        }

        private bool isWhiteListOpen;
        public bool IsWhiteListOpen
        {
            get => isWhiteListOpen;
            set => SetProperty(ref isWhiteListOpen, value);
        }
        

        private bool isAlwaysOnOpen;
        public bool IsAlwaysOnOpen
        {
            get => isAlwaysOnOpen;
            set => SetProperty(ref isAlwaysOnOpen, value);
        }


        private readonly IRdpService rdpService;
        public IndicatorsUserControlViewModel(IRdpService rdpService)
        {
            this.rdpService = rdpService;
            rdpService.RdpDataUpdated += UpdateIndicators;
            rdpService.RefreshRdpData();
        }

        private void UpdateIndicators()
        {
            Debug.WriteLine(rdpService.RdpData.IsRoleActive);
            Debug.WriteLine(rdpService.RdpData.IsOpenForAll);
            Debug.WriteLine(rdpService.RdpData.IsOpenForAlwaysOnList);
            Debug.WriteLine(rdpService.RdpData.IsOpenForLocalComputers);
            Debug.WriteLine(rdpService.RdpData.IsOpenForLocalComputersAndForWhiteList);


            if (rdpService.RdpData.IsOpenForAll)
            {
                IsAlwaysOnOpen = true;
                IsInternalOpen = true;
                IsWhiteListOpen = true;
                IsExternalOpen = true;
            }
            else if (rdpService.RdpData.IsOpenForLocalComputersAndForWhiteList)
            {
                IsExternalOpen = false;
                IsWhiteListOpen = true;
                IsInternalOpen = true;
            }
            else if (rdpService.RdpData.IsOpenForLocalComputers)
            {
                IsAlwaysOnOpen = true;
                IsInternalOpen = true;
                IsWhiteListOpen = false;
                IsExternalOpen = false;
            }

            if (rdpService.RdpData.IsOpenForAlwaysOnList)
            {
                IsAlwaysOnOpen = true;
            }

            if (!rdpService.RdpData.IsRoleActive)
            {
                IsAlwaysOnOpen = false;
                IsInternalOpen = false;
                IsWhiteListOpen = false;
                IsExternalOpen = false;
            }
                IsAlwaysOnOpen = rdpService.RdpData.IsOpenForAlwaysOnList;
                IsInternalOpen = rdpService.RdpData.IsOpenForLocalComputers;
                IsWhiteListOpen = rdpService.RdpData.IsOpenForLocalComputersAndForWhiteList;
                IsExternalOpen = rdpService.RdpData.IsOpenForAll;
        }
    }
}
