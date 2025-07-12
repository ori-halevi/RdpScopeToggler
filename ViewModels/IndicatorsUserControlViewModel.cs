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
                IsAlwaysOnOpen = rdpService.RdpData.IsOpenForAlwaysOnList;
                IsInternalOpen = rdpService.RdpData.IsOpenForLocalComputers;
                IsWhiteListOpen = rdpService.RdpData.IsOpenForWhiteList;
                IsExternalOpen = rdpService.RdpData.IsOpenForAll;
        }
    }
}
