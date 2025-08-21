using RdpScopeCommands.Stores;
using RdpScopeToggler.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RdpScopeToggler.Services.PipeClientService
{
    public interface IPipeClientService
    {
        event Action<string> MessageReceived;

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task SendAsync(string message, CancellationToken cancellationToken = default);
        void SendAddTask(ActionsEnum action, DateTime date);
        RdpTask? GetUpcomingTask();
        bool IsConnected { get; }
    }
}
