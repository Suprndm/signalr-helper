using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRHelper.WebServerTester
{
    public class Client
    {
        public Client(string id)
        {
            Id = id;
            LastPongTime = DateTimeOffset.MinValue;
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public string Id { get; }
        public DateTimeOffset LastPongTime { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
