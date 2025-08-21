using RdpScopeToggler.Services.RdpService;
using Prism.Mvvm;
using System.Diagnostics;
using RdpScopeCommands;
using RdpScopeToggler.Services.PipeClientService;
using System;
using System.Windows;
using System.Threading;
using System.Text.Json;
using RdpScopeToggler.Stores;
using System.Text.Json.Serialization;
using RdpScopeCommands.Stores;
using Prism.Navigation.Regions;
using Prism.Navigation;
using System.Linq;

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
            public RdpTask[] TasksArr { get; set; } = Array.Empty<RdpTask>();
            public RdpInfoData CurrentRdpState { get; set; } = new RdpInfoData();
            public DateTime TimeStemp { get; set; } = DateTime.UtcNow;
        }
        private readonly IRdpController rdpService;
        private readonly IPipeClientService pipeClientService;
        private IRegionManager regionManager;
        public IndicatorsUserControlViewModel(IRegionManager regionManager, IRdpController rdpService, IPipeClientService pipeClientService)
        {
            this.regionManager = regionManager;
            this.rdpService = rdpService;
            rdpService.RdpDataUpdated += UpdateIndicators;
            rdpService.RefreshRdpData();

            this.pipeClientService = pipeClientService;
            this.pipeClientService.MessageReceived += OnMessageReceived;
            _ = this.pipeClientService.ConnectAsync(CancellationToken.None);

        }

        private void OnMessageReceived(string obj)
        {
            Debug.WriteLine("Got new message...");
            Debug.WriteLine(obj);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(),
                }
            };

            // אין צורך ב-JsonDocument אם JSON כבר מייצג את ServiceMessage
            var messageAsJson = JsonSerializer.Deserialize<ServiceMessage>(obj, options);

            //CheckIfNeed(messageAsJson.TasksArr[0]);
            rdpService.RefreshRdpData();
        }

        private void UpdateIndicators()
        {
            Debug.WriteLine($"Update Indicators...");
            IsAlwaysOnOpen = rdpService.RdpData.IsOpenForAlwaysOnList;
            IsInternalOpen = rdpService.RdpData.IsOpenForLocalComputers;
            IsWhiteListOpen = rdpService.RdpData.IsOpenForWhiteList;
            IsExternalOpen = rdpService.RdpData.IsOpenForAll;
        }
/*        private void CheckIfNeed(RdpTask task)
         {
            if (task != null)
            {
                if (task.Action == ActionsEnum.CloseRdp)
                {
                    var closeRdpTask = tasks.FirstOrDefault(t => t.State == StateEnum.InQueue && t.Action == ActionsEnum.CloseRdp);
                    var parameters1 = new NavigationParameters
                    {
                        { "task", closeRdpTask }
                    };
                    regionManager.RequestNavigate("ActionsRegion", "TaskUserControl", parameters1);
                }
                else if (task.Action == ActionsEnum.LocalComputersAndWhiteList)
                {
                    var closeRdpTask = tasks.FirstOrDefault(t => t.State == StateEnum.InQueue && t.Action == ActionsEnum.LocalComputersAndWhiteList);
                    var parameters4 = new NavigationParameters
                    {
                        { "task", closeRdpTask }
                    };
                    regionManager.RequestNavigate("ActionsRegion", "TaskUserControl", parameters4);
                }
                else if (task.Action == ActionsEnum.RemoteSystems)
                {
                    var remoteSystemsTask = tasks.FirstOrDefault(t => t.State == StateEnum.InQueue && t.Action == ActionsEnum.RemoteSystems);
                    var closeRdpTask = tasks.FirstOrDefault(t => t.State == StateEnum.InQueue && t.Action == ActionsEnum.LocalComputersAndWhiteList);
                    var parameters2 = new NavigationParameters
                    {
                        { "mainTask", remoteSystemsTask },
                        { "closeRdpTask", closeRdpTask }
                    };
                    regionManager.RequestNavigate("ActionsRegion", "WaitingUserControl", parameters2);
                }
                else if (task.Action == ActionsEnum.WhiteList)
                {
                    var closeRdpTask = tasks.FirstOrDefault(t => t.State == StateEnum.InQueue && t.Action == ActionsEnum.LocalComputersAndWhiteList);
                    var whiteListTask = tasks.FirstOrDefault(t => t.State == StateEnum.InQueue && t.Action == ActionsEnum.WhiteList);
                    var parameters3 = new NavigationParameters
                    {
                        { "mainTask", whiteListTask },
                        { "closeRdpTask", closeRdpTask }
                    };
                    regionManager.RequestNavigate("ActionsRegion", "WaitingUserControl", parameters3);
                }
            }
        }
*/
    }
}
