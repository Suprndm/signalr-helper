using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRHelper.Client
{
    public class SignalRClient : ISignalRClient
    {
        private readonly string _name;
        private readonly string _baseUrl;
        private readonly TimeSpan _connectionTimeoutDelay;
        private readonly TimeSpan _connectionDisturbedDelay;
        private readonly TimeSpan _connectionRefreshFrequency;
        private readonly TimeSpan _connectionRetryDelay;
        private readonly TimeSpan _pingPongFrequency;
        private ConnectionStatus _connectionStatus;

        private Timer _connectionRefreshTimer;
        private Timer _connectionRetryTimer;

        private DateTimeOffset _lastPingTime;
        private TimeSpan _previousDelayBetweenPings;

        public SignalRClient(
            string name, 
            string baseUrl,
            int connectionRetryDelayMs = 5000,
            int connectionTimeoutDelayMs = 6000,
            int connectionDisturbedDelayMs = 3000, 
            int connectionRefreshFrequencyMs = 1000, 
            int pingPongFrequencyMs = 1000)
        {
            _name = name;
            _baseUrl = baseUrl;
            _connectionRetryDelay = TimeSpan.FromMilliseconds(connectionRetryDelayMs);
            _connectionTimeoutDelay = TimeSpan.FromMilliseconds(connectionTimeoutDelayMs);
            _connectionDisturbedDelay = TimeSpan.FromMilliseconds(connectionDisturbedDelayMs);
            _connectionRefreshFrequency = TimeSpan.FromMilliseconds(connectionRefreshFrequencyMs);
            _pingPongFrequency = TimeSpan.FromMilliseconds(pingPongFrequencyMs);
        }

        public void Connect()
        {
            _lastPingTime = DateTimeOffset.MinValue;
            _previousDelayBetweenPings = TimeSpan.FromDays(1);

            _connectionStatus = ConnectionStatus.Disconnected;

            _connectionRetryTimer = new Timer((e) => AttemptConnectionAsync(), null, TimeSpan.FromMilliseconds(0), _connectionRetryDelay);
        }

        private async void AttemptConnectionAsync()
        {
            HubConnection = new HubConnectionBuilder()
                .WithUrl(_baseUrl)
                .Build();

            try
            {
                await HubConnection.StartAsync();

                Connected?.Invoke();

                _connectionRetryTimer.Dispose();


                HubConnection.On("Ping", async () =>
                {
                    _previousDelayBetweenPings = DateTimeOffset.UtcNow - _lastPingTime;
                    _lastPingTime = DateTimeOffset.UtcNow;
                    await Task.Delay(_pingPongFrequency);
                    try
                    {
                        await HubConnection.SendAsync("Pong");
                    }
                    catch (Exception e)
                    {
                        ExceptionOccured?.Invoke(new SignalRClientException(
                            $"Pong to {_baseUrl} failed. Will try to reconnect in {_connectionRetryDelay.TotalMilliseconds}ms. Reason :", e));
                    }
                 
                });

                _connectionRefreshTimer = new Timer((e) => RefreshConnectionStatus(), null, TimeSpan.FromMilliseconds(0), _connectionRefreshFrequency);
            }
            catch (Exception e)
            {
                ExceptionOccured?.Invoke(new SignalRClientException(
                    $"Connection to {_baseUrl} failed. Will retry in {_connectionRetryDelay.TotalMilliseconds}ms. Reason :", e));
            }
        }

        private void RefreshConnectionStatus()
        {
            var timeElapsedTillLastPong = DateTimeOffset.UtcNow - _lastPingTime;

            var maxDelay = Math.Max(timeElapsedTillLastPong.TotalMilliseconds,
                _previousDelayBetweenPings.TotalMilliseconds);

            var connectionStatus = ComputeConnectionStatusFromDelay(TimeSpan.FromMilliseconds(maxDelay));

            UpdateConnectionStatus(connectionStatus);
        }

        private ConnectionStatus ComputeConnectionStatusFromDelay(TimeSpan delay)
        {
            if (delay > _connectionTimeoutDelay)
            {
                return ConnectionStatus.Disconnected;
            }

            if (delay > _connectionDisturbedDelay)
            {
                return ConnectionStatus.Disturbed;
            }

            return ConnectionStatus.Healthy;
        }


        private void UpdateConnectionStatus(ConnectionStatus newConnectionStatus)
        {
            if (_connectionStatus == newConnectionStatus) return;
            _connectionStatus = newConnectionStatus;
            ConnectionStatusChanged?.Invoke(_connectionStatus);

            if (_connectionStatus == ConnectionStatus.Disconnected)
            {
                _connectionRefreshTimer.Dispose();
                Connect();
            }
        }

        public async Task DisconnectAsync()
        {
            _connectionRefreshTimer.Dispose();
            _connectionRetryTimer.Dispose();
           await HubConnection.DisposeAsync();
        }

        public HubConnection HubConnection { get; private set; }

        public ConnectionStatus GetConnectionStatus()
        {
            return _connectionStatus;
        }

        public event Action Connected;
        public event Action Disconnected;
        public event Action<ConnectionStatus> ConnectionStatusChanged;
        public event Action<SignalRClientException> ExceptionOccured;
    }
}
