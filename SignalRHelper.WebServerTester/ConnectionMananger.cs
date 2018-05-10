using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SignalRHelper.WebServerTester
{
    public class ConnectionMananger
    {
        private readonly IList<Client> _clients;
        private static ConnectionMananger _instance;
        private TimeSpan _healthyDelayBetweenPongs = TimeSpan.FromMilliseconds(3000);
        private TimeSpan _disturbedDelayBetweenPongs = TimeSpan.FromMilliseconds(6000);
        private TimeSpan _connectionsRefreshFrequency = TimeSpan.FromMilliseconds(1000);

        private Timer _timer;

        private ConnectionMananger()
        {
            _clients = new List<Client>();
            _timer = new Timer(e=> RefreshConnectionsStatus(), null, TimeSpan.FromMilliseconds(0), _connectionsRefreshFrequency);
        }

        public event Action<Client> ClientConnected;
        public event Action<Client> ClientDisconnected;
        public event Action<Client> ClientConnectionStatusChanged;

        public static ConnectionMananger Instance => _instance ?? (_instance = new ConnectionMananger());

        public void UpdateSettings(TimeSpan healthyDelayBetweenPongs, TimeSpan disturbedDelayBetweenPongs,
            TimeSpan connectionsRefreshFrequency)
        {
            _timer.Dispose();

            _healthyDelayBetweenPongs = healthyDelayBetweenPongs;
            _disturbedDelayBetweenPongs = disturbedDelayBetweenPongs;
            _connectionsRefreshFrequency = connectionsRefreshFrequency;

            _timer = new Timer(e => RefreshConnectionsStatus(), null, TimeSpan.FromMilliseconds(0), _connectionsRefreshFrequency);
        }

        public void OnClientConnected(string clientId)
        {
            var client = new Client(clientId);
            _clients.Add(client);
            ClientConnected?.Invoke(client);
        }

        public void OnClientDisconnected(string clientId)
        {
            var client = GetClientById(clientId);
            _clients.Remove(client);
            ClientDisconnected?.Invoke(client);
        }

        public void OnPongReceived(string clientId)
        {
            var client = GetClientById(clientId);
            client.LastPongTime = DateTimeOffset.UtcNow;
        }

        public ConnectionStatus GetConnectionStatus(string clientId)
        {
            return GetClientById(clientId).ConnectionStatus;
        }

        private void RefreshConnectionsStatus()
        {
            foreach (var client in _clients.ToList())
            {
                RefreshConnectionStatus(client);
            }
        }

        private void RefreshConnectionStatus(Client client)
        {
            var delayBetweenPongs = DateTimeOffset.UtcNow - client.LastPongTime;
            if (delayBetweenPongs <= _healthyDelayBetweenPongs)
                UpdateClientConnectionStatus(client, ConnectionStatus.Healthy);
            else if (delayBetweenPongs <= _disturbedDelayBetweenPongs)
            {
                UpdateClientConnectionStatus(client, ConnectionStatus.Disturbed);
            }
            else
            {
                UpdateClientConnectionStatus(client, ConnectionStatus.Disconnected);
            }
        }

        private void UpdateClientConnectionStatus(Client client, ConnectionStatus newConnectionStatus)
        {
            if (client.ConnectionStatus == newConnectionStatus) return;

            client.ConnectionStatus = newConnectionStatus;
            ClientConnectionStatusChanged?.Invoke(client);
        }

        private Client GetClientById(string clientId)
        {
            return _clients.Single(client => client.Id == clientId);
        } 
    }
}
