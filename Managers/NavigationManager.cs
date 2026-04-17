using Prism.Navigation;
using Prism.Navigation.Regions;
using RdpScopeToggler.Models;
using RdpScopeToggler.Services.PipeClientService;
using System.Windows;

namespace RdpScopeToggler.Managers
{
    public class NavigationManager
    {
        private readonly IRegionManager regionManager;
        public NavigationManager(IPipeClientService pipeClientService, IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            pipeClientService.MessageReceived += UpdateNavigate;
        }

        private void UpdateNavigate(ServiceMessage message)
        {
            RdpTask currentTask = message.CurrentTask;

            if (currentTask == null)
                return;

            // Pipe messages arrive on a background thread. During app shutdown
            // Application.Current may be null or the Dispatcher may be shutting down.
            var app = Application.Current;
            if (app?.Dispatcher == null || app.Dispatcher.HasShutdownStarted)
                return;

            var parameters = new NavigationParameters
            {
                { "task", currentTask }
            };

            if (currentTask.State == StateEnum.Executed && currentTask.NextTask != null && currentTask.NextTask.State == StateEnum.Executed)
            {
                app.Dispatcher.Invoke(() =>
                {
                    regionManager.RequestNavigate("ActionsRegion", "HomeUserControl", parameters);
                });
            }
            else if (currentTask.State == StateEnum.Executed && currentTask.NextTask != null && currentTask.NextTask.State == StateEnum.InQueue)
            {
                app.Dispatcher.Invoke(() =>
                {
                    regionManager.RequestNavigate("ActionsRegion", "TaskUserControl", parameters);
                });
            }
            else if (currentTask.State == StateEnum.InQueue && currentTask.NextTask != null && currentTask.NextTask.State == StateEnum.InQueue)
            {
                app.Dispatcher.Invoke(() =>
                {
                    regionManager.RequestNavigate("ActionsRegion", "WaitingUserControl", parameters);
                });
            }
            else
            {
                app.Dispatcher.Invoke(() =>
                {
                    regionManager.RequestNavigate("ActionsRegion", "HomeUserControl", parameters);
                });
            }
        }
    }
}
