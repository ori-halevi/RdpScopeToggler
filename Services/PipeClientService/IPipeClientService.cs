using RdpScopeCommands.Stores;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RdpScopeToggler.ViewModels.IndicatorsUserControlViewModel;

namespace RdpScopeToggler.Services.PipeClientService
{
    public interface IPipeClientService
    {
        event Action<ServiceMessage> MessageReceived;

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
        Task AskForUpdate();
        Task SendAsync(string message, CancellationToken cancellationToken = default);
        void SendAddTask(RdpTask taskToAdd);
        void ForceExecuteTask(string taskId);
        void SendRemoveTask(string taskId);
        RdpTask? GetUpcomingTask();
        bool IsConnected { get; }
    }
}
