using System;

namespace SignalRHelper.Server
{
    public class Client
    {
        public Client(string id)
        {
            Id = id;
            LastPongTime = DateTimeOffset.MinValue;

            // Represents a big delay
            PreviousDelayBetweenPongs =  TimeSpan.FromDays(1);
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public string Id { get; }
        public DateTimeOffset LastPongTime { get; set; }
        public TimeSpan PreviousDelayBetweenPongs { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
