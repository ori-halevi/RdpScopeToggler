using RdpScopeToggler.Models;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.PipeClientService
{
    public interface IPipeClientService
    {
        event Action<ServiceMessage>? MessageReceived;
        event Action<List<Client>>? WhiteListReceived;
        event Action<List<Client>>? AlwaysTrustedListReceived;

        CancellationTokenSource Cts { get; set; }

        bool IsConnected { get; }

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task AskForUpdate();
        Task SendAsync(string message, CancellationToken cancellationToken = default);

        // Fire-and-forget methods
        void SendAddTask(RdpTask taskToAdd);
        void SendRemoveTask(string taskId);

        // Asynchronous version of GetUpcomingTask to avoid blocking and not close the main pipe
        Task<RdpTask?> GetUpcomingTaskAsync();

        Task SendUpdateAlwaysTrustedList(List<Client> clients);
        Task SendUpdateWhiteList(List<Client> clients);

        Task AskWhiteListUpdate();
        Task AskAlwaysTrustedListUpdate();
    }
}

