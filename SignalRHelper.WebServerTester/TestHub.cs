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
        private readonly TimeSpan _pingFrequency = TimeSpan.FromMilliseconds(1000);
        private readonly Logger _logger;

        public TestHub(Logger logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.Log("connected client");
            await base.OnConnectedAsync();
        }
    }
}
