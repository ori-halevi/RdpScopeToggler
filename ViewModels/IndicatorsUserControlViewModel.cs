using RdpScopeToggler.Services.RdpService;
using Prism.Mvvm;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace RdpScopeToggler.ViewModels
{
    public class IndicatorsUserControlViewModel : BindableBase
    {
        private bool _isInternalOpen = true;
        public bool IsInternalOpen
        {
            get => _isInternalOpen;
            set => SetProperty(ref _isInternalOpen, value);
        }

        private bool _isExternalOpen;
        public bool IsExternalOpen
        {
            get => _isExternalOpen;
            set => SetProperty(ref _isExternalOpen, value);
        }

        private bool _isWhiteListOpen;
        public bool IsWhiteListOpen
        {
            get => _isWhiteListOpen;
            set => SetProperty(ref _isWhiteListOpen, value);
        }


        private readonly IRdpService rdpService;
        public IndicatorsUserControlViewModel(IRdpService rdpService)
        {
            this.rdpService = rdpService;
            rdpService.RdpDataUpdated += UpdateIndicators;
        }

        private void UpdateIndicators()
        {
            if (rdpService.RdpData.IsOpenForAll)
            {
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
            else if (rdpService.RdpData.IsOpenOnlyForLocal)
            {
                IsInternalOpen = true;
                IsWhiteListOpen = false;
                IsExternalOpen = false;
            }

            if (!rdpService.RdpData.IsRoleActive)
            {
                IsInternalOpen = false;
                IsWhiteListOpen = false;
                IsExternalOpen = false;
            }
        }
    }
}
