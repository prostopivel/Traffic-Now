using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Text.Json;
using Traffic.API.Contracts;
using Traffic.Core.Abstractions.Services;

namespace Traffic.API.APIServices
{
    public class TransportAPIConnection : ITransportAPIConnection
    {
        private readonly ConcurrentDictionary<string, HubConnection> _connections = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastDataReceivedTimes = new();
        private readonly ILogger<TransportAPIConnection> _logger;

        public TransportAPIConnection(ILogger<TransportAPIConnection> logger)
        {
            _logger = logger;
        }

        public string GetConnectionUrl(string hubUrl)
        {
            if (!hubUrl.StartsWith("http://") && !hubUrl.StartsWith("https://"))
            {
                hubUrl = "http://" + hubUrl;
            }

            var uriBuilder = new UriBuilder(hubUrl);
            if (!uriBuilder.Path.EndsWith("/transportHub"))
            {
                uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + "/transportHub";
            }

            var finalUrl = uriBuilder.Uri.ToString();

            return finalUrl;
        }

        public async Task<string?> AddConnectionAsync(string connectionId, string hubUrl)
        {
            if (_connections.ContainsKey(connectionId))
            {
                _logger.LogWarning("Connection with ID {ConnectionId} already exists", connectionId);
                return null;
            }

            var finalUrl = GetConnectionUrl(hubUrl);

            var hubConnection = new HubConnectionBuilder()
                .WithUrl(finalUrl)
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On<string>("TransportData", (jsonData) =>
            {
                _lastDataReceivedTimes[connectionId] = DateTime.UtcNow;
                HandleReceivedData(connectionId, finalUrl, jsonData);
            });

            try
            {
                await hubConnection.StartAsync();
                _connections[connectionId] = hubConnection;

                _lastDataReceivedTimes[connectionId] = DateTime.UtcNow;

                return finalUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to connect to hub {HubUrl}. Message: {message}", finalUrl, ex.Message);
                return null;
            }
        }

        public HubConnection? GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var connection);
            return connection;
        }

        public IEnumerable<string> GetActiveConnectionIds()
        {
            return _connections.Keys;
        }

        public async Task StopAllConnectionsAsync()
        {
            foreach (var (connectionId, connection) in _connections)
            {
                try
                {
                    await connection.StopAsync();
                    await connection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping connection {ConnectionId}", connectionId);
                }
            }
            _connections.Clear();
            _lastDataReceivedTimes.Clear();
        }

        public async Task<bool> RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var connection))
            {
                try
                {
                    await connection.StopAsync();
                    await connection.DisposeAsync();
                    _lastDataReceivedTimes.TryRemove(connectionId, out _);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing connection {ConnectionId}", connectionId);
                }
            }
            return false;
        }

        public bool IsTransportActive(string connectionId, int timeoutSeconds = 10)
        {
            if (!_connections.ContainsKey(connectionId))
            {
                _logger.LogWarning("Connection {ConnectionId} not found", connectionId);
                return false;
            }

            if (!_lastDataReceivedTimes.TryGetValue(connectionId, out var lastReceivedTime))
            {
                _logger.LogWarning("No data received time for connection {ConnectionId}", connectionId);
                return false;
            }

            var timeSinceLastData = DateTime.UtcNow - lastReceivedTime;
            bool isActive = timeSinceLastData.TotalSeconds <= timeoutSeconds;

            _logger.LogDebug("Transport {ConnectionId} check: {TimeSinceLastData:F1}s, Active: {IsActive}",
                connectionId, timeSinceLastData.TotalSeconds, isActive);

            return isActive;
        }

        private void HandleReceivedData(string connectionId, string hubUrl, string jsonData)
        {
            try
            {
                var data = JsonSerializer.Deserialize<PointTransportResponse>(jsonData);
                //_logger.LogInformation("Received data from {HubUrl} for transport {TransportId}",
                //    hubUrl, data?.TransportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing JSON from {HubUrl}", hubUrl);
            }
        }
    }
}