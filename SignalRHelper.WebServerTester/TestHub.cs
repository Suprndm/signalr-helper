using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRHelper.WebServerTester
{
    public class TestHub : Microsoft.AspNetCore.SignalR.Hub
    {

        private readonly TimeSpan _pingFrequency = TimeSpan.FromMilliseconds(1000);
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
            await Ping();
        }

        public async Task Pong()
        {
            await Ping();
        }

        private async Task Ping()
        {
            await Task.Delay(_pingFrequency);
            await Clients.Caller.SendAsync("Ping");
        }
    }
}
