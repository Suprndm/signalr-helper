using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalRHelper.Server;

namespace SignalRHelper.WebServerTester
{
    public class TestHub : ManagedHub
    {
        private readonly Logger _logger;

        public TestHub(Logger logger)
        {
            _logger = logger;
            PingFrequency = TimeSpan.FromMilliseconds(0);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.Log("connected client");
            await base.OnConnectedAsync();
        }
    }
}
