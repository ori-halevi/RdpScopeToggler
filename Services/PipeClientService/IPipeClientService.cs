using RdpScopeToggler.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.PipeClientService
{
    public interface IPipeClientService
    {
        event Action<ServiceMessage> MessageReceived;

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
        Task AskForUpdate();
        Task SendAsync(string message, CancellationToken cancellationToken = default);
        void SendAddTask(RdpTask taskToAdd);
        void SendRemoveTask(string taskId);
        RdpTask? GetUpcomingTask();
        bool IsConnected { get; }
    }
}
