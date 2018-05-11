using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace SignalRHelper.Server.Tests
{
    public class HubConnectionManagerTests
    {

        private IDummyHubConnectionManagerSubscriber _eventTester;

        [SetUp]
        public void Setup()
        {
            HubConnectionManager.Instance.UpdateSettings(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(100));

            _eventTester = Substitute.For<IDummyHubConnectionManagerSubscriber>();

            HubConnectionManager.Instance.ClientConnected += _eventTester.ClientConnected;
            HubConnectionManager.Instance.ClientDisconnected += _eventTester.ClientDisconnected;
            HubConnectionManager.Instance.ClientConnectionStatusChanged += _eventTester.ClientConnectionStatusChanged;
        }

        [Test]
        public void ShouldNotifyOnClientConnection()
        {
            // When a client connects
            HubConnectionManager.Instance.OnClientConnected("a");

            // Then ClientConnected event should be fired
            _eventTester.Received(1).ClientConnected(Arg.Is<Client>(c =>
                c.Id == "a"
                && c.ConnectionStatus == ConnectionStatus.Disconnected
                && c.LastPongTime == DateTimeOffset.MinValue));

            HubConnectionManager.Instance.OnClientDisconnected("a");
        }

        [Test]
        public void ShouldNotifyOnClientDisconnection()
        {
            // given a connected Client
            HubConnectionManager.Instance.OnClientConnected("b");

            // When the client disconnects
            HubConnectionManager.Instance.OnClientDisconnected("b");

            // Then Client Disconnected event should be fired
            _eventTester.Received(1).ClientDisconnected(Arg.Is<Client>(c =>
                c.Id == "b"
                && c.ConnectionStatus == ConnectionStatus.Disconnected
                && c.LastPongTime == DateTimeOffset.MinValue));
        }


        [Test]
        public async Task ShouldChangeConnectionStatusOfClientToHealthyWhenUnderHealthDelayLimit()
        {
            // given a connectedClient
            HubConnectionManager.Instance.OnClientConnected("c");

            // When the client send pongs under healthy delay 
            HubConnectionManager.Instance.OnPongReceived("c");
            await Task.Delay(80);
            HubConnectionManager.Instance.OnPongReceived("c");
            await Task.Delay(80);
            HubConnectionManager.Instance.OnPongReceived("c");
            await Task.Delay(80);

            // Then ClientConnectionStatusChanged event should be fired with healthyStatus
            _eventTester.Received(1).ClientConnectionStatusChanged(Arg.Is<Client>(c =>
                c.Id == "c"
                && c.ConnectionStatus == ConnectionStatus.Healthy
                && c.LastPongTime != DateTimeOffset.MinValue));
            HubConnectionManager.Instance.OnClientDisconnected("c");
        }

        [Test]
        public async Task ShouldChangeConnectionStatusOfClientToDisturbedWhenUnderDisturbedDelayLimit()
        {
            // given a connectedClient
            HubConnectionManager.Instance.OnClientConnected("d");

            // When the client send pongs under healthy delay 
            HubConnectionManager.Instance.OnPongReceived("d");
            await Task.Delay(210);
            HubConnectionManager.Instance.OnPongReceived("d");
            await Task.Delay(210);
            HubConnectionManager.Instance.OnPongReceived("d");
            await Task.Delay(210);

            // Then ClientConnectionStatusChanged event should be fired with healthyStatus
            _eventTester.Received(1).ClientConnectionStatusChanged(Arg.Is<Client>(c =>
                c.Id == "d"
                && c.ConnectionStatus == ConnectionStatus.Disturbed
                && c.LastPongTime != DateTimeOffset.MinValue));
            HubConnectionManager.Instance.OnClientDisconnected("d");
        }


        [Test]
        public async Task ShouldChangeConnectionStatusOfClientToDisconnectedWhenOverDisturbedDelayLimit()
        {
            // given a connectedClient
            HubConnectionManager.Instance.OnClientConnected("e");

            // When the client send pongs under healthy delay 
            HubConnectionManager.Instance.OnPongReceived("e");
            await Task.Delay(50);
            HubConnectionManager.Instance.OnPongReceived("e");
            await Task.Delay(100);
            HubConnectionManager.Instance.OnPongReceived("e");
            await Task.Delay(500);

            // Then client disconnected event should be fired
            _eventTester.Received(1).ClientDisconnected(Arg.Any<Client>());
        }
    }

    public interface IDummyHubConnectionManagerSubscriber
    {
        void ClientConnected(Client client);
        void ClientDisconnected(Client client);
        void ClientConnectionStatusChanged(Client client);
    }
}
