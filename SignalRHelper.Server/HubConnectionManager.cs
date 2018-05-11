using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SignalRHelper.Server
{
    public class HubConnectionManager
    {
        private readonly IList<Client> _clients;
        private static HubConnectionManager _instance;
        private TimeSpan _healthyDelayBetweenPongs = TimeSpan.FromMilliseconds(3000);
        private TimeSpan _disturbedDelayBetweenPongs = TimeSpan.FromMilliseconds(6000);
        private TimeSpan _connectionsRefreshFrequency = TimeSpan.FromMilliseconds(1000);

        private Timer _connectionRefreshTimer;

        public event Action<Client> ClientConnected;
        public event Action<Client> ClientDisconnected;
        public event Action<Client> ClientConnectionStatusChanged;

        private HubConnectionManager()
        {
            _clients = new List<Client>();
            _connectionRefreshTimer = new Timer(e => RefreshConnectionsStatus(), null, TimeSpan.FromMilliseconds(0), _connectionsRefreshFrequency);
        }


        public static HubConnectionManager Instance => _instance ?? (_instance = new HubConnectionManager());


        public void UpdateSettings(TimeSpan healthyDelayBetweenPongs, TimeSpan disturbedDelayBetweenPongs,
            TimeSpan connectionsRefreshFrequency)
        {
            _connectionRefreshTimer.Dispose();

            _healthyDelayBetweenPongs = healthyDelayBetweenPongs;
            _disturbedDelayBetweenPongs = disturbedDelayBetweenPongs;
            _connectionsRefreshFrequency = connectionsRefreshFrequency;

            _connectionRefreshTimer = new Timer(e => RefreshConnectionsStatus(), null, TimeSpan.FromMilliseconds(0), _connectionsRefreshFrequency);
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
            client.PreviousDelayBetweenPongs = DateTimeOffset.UtcNow - client.LastPongTime;
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
            var timeElapsedTillLastPong = DateTimeOffset.UtcNow - client.LastPongTime;

            var maxDelay = Math.Max(timeElapsedTillLastPong.TotalMilliseconds,
                client.PreviousDelayBetweenPongs.TotalMilliseconds);

            var connectionStatus = ComputeConnectionStatusFromDelay(TimeSpan.FromMilliseconds(maxDelay));

            UpdateClientConnectionStatus(client, connectionStatus);
        }

        private ConnectionStatus ComputeConnectionStatusFromDelay(TimeSpan delay)
        {
            if (delay > _disturbedDelayBetweenPongs)
            {
                return ConnectionStatus.Disconnected;
            }

            if (delay > _healthyDelayBetweenPongs)
            {
                return ConnectionStatus.Disturbed;
            }

            return ConnectionStatus.Healthy;
        }

        private void UpdateClientConnectionStatus(Client client, ConnectionStatus newConnectionStatus)
        {
            lock (client)
            {
                if (client.ConnectionStatus == newConnectionStatus) return;

                client.ConnectionStatus = newConnectionStatus;
                ClientConnectionStatusChanged?.Invoke(client);

                if (newConnectionStatus == ConnectionStatus.Disconnected)
                {
                    OnClientDisconnected(client.Id);
                }
            }
        }

        private Client GetClientById(string clientId)
        {
            return _clients.Single(client => client.Id == clientId);
        }
    }
}
