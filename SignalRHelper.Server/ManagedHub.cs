using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRHelper.Server
{
    public class ManagedHub : Hub
    {
        protected TimeSpan PingFrequency = TimeSpan.FromMilliseconds(1000);

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
            await Ping();

            HubConnectionManager.Instance.OnClientConnected(Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            HubConnectionManager.Instance.OnClientDisconnected(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Pong()
        {
            HubConnectionManager.Instance.OnPongReceived(Context.ConnectionId);
            await Ping();
        }

        private async Task Ping()
        {
            await Task.Delay(PingFrequency);
            await Clients.Caller.SendAsync("Ping");
        }
    }
}
