using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRHelper.Client
{
    public interface ISignalRClient
    {
        void Connect();
        Task DisconnectAsync();

        HubConnection HubConnection { get; }
        ConnectionStatus GetConnectionStatus();

        event Action Connected;
        event Action Disconnected;
        event Action<ConnectionStatus> ConnectionStatusChanged;
        event Action<SignalRClientException> ExceptionOccured;
    }
}
