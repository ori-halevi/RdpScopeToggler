using Prism.Mvvm;
using Prism.Navigation.Regions;
using RdpScopeCommands.Stores;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.Stores;
using System;
using System.Diagnostics;
using System.Threading;

namespace RdpScopeToggler.ViewModels
{
    public class IndicatorsUserControlViewModel : BindableBase
    {
        #region Properties
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
        #endregion
        public class ServiceMessage
        {
            public RdpTask CurrentTask { get; set; } = new RdpTask();
            public RdpInfoData CurrentRdpState { get; set; } = new RdpInfoData();
            public DateTime TimeStemp { get; set; } = DateTime.UtcNow;
        }

        private readonly IPipeClientService pipeClientService;
        private IRegionManager regionManager;
        public IndicatorsUserControlViewModel(IRegionManager regionManager, IPipeClientService pipeClientService)
        {
            this.regionManager = regionManager;

            this.pipeClientService = pipeClientService;
            this.pipeClientService.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(ServiceMessage message)
        {
            Debug.WriteLine("Got new message...");

            // Update the indicators
            UpdateIndicators(message.CurrentRdpState);
        }

        private void UpdateIndicators(RdpInfoData rdpInfoData)
        {
            Debug.WriteLine($"Update Indicators...");
            IsAlwaysOnOpen = rdpInfoData.IsOpenForAlwaysOnList;
            IsInternalOpen = rdpInfoData.IsOpenForLocalComputers;
            IsWhiteListOpen = rdpInfoData.IsOpenForWhiteList;
            IsExternalOpen = rdpInfoData.IsOpenForAll;
        }
    }
}
