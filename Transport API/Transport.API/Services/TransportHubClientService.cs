using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text;
using System.Text.Json;
using Transport.API.Contracts;
using Transport.Core.Abstractions;

namespace Transport.API.Services
{
    public class TransportHubClientService : BackgroundService
    {
        private readonly HubConnection _hubConnection;
        private readonly IDataService _dataService;
        private readonly ILogger<TransportHubClientService> _logger;
        private readonly string _serverId, _serverUrl;

        public TransportHubClientService(
            IConfiguration configuration,
            IDataService dataService,
            ILogger<TransportHubClientService> logger)
        {
            _dataService = dataService;
            _logger = logger;

            var hubUrl = configuration["CentralHubUrl"] ?? "https://localhost:7003/transportAPIHub";
            _serverUrl = hubUrl.Replace("/transportAPIHub", "/api/transport/getData");
            _serverId = configuration["Kestrel:Endpoints:Http:Url"] ?? Guid.NewGuid().ToString();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            ConfigureHubEvents();
        }

        private void ConfigureHubEvents()
        {
            _hubConnection.On("TransportDataRequest", SendTransportData);

            _hubConnection.Closed += async (error) =>
            {
                _logger.LogWarning("Connection closed: {Error}", error?.Message);
                await Task.Delay(5000);
                await ConnectToHub();
            };

            _hubConnection.Reconnecting += (error) =>
            {
                _logger.LogWarning("Reconnecting: {Error}", error?.Message);
                return Task.CompletedTask;
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectToHub();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ConnectToHub()
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();

                    await _hubConnection.InvokeAsync("SubscribeAsTransportServer", _serverId);

                    _logger.LogInformation("Connected to central hub!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to central hub: {Error}", ex.Message);
            }
        }

        private async Task SendTransportData()
        {
            try
            {
                if (_dataService.Transport == null || !_dataService.IsActive)
                {
                    return;
                }

                var data = new PointResponse(
                    _dataService.Transport.Id,
                    _dataService.Map.Id,
                    _dataService.Transport.X,
                    _dataService.Transport.Y);

                string jsonData = JsonSerializer.Serialize(data);

                using var httpClient = new HttpClient();
                HttpResponseMessage? response = null;
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(_serverUrl, content);
                    _logger.LogInformation("Sent data to central hub: {Data}", jsonData);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Can not connect to {url}! Error message: {message}", _serverUrl, ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending transport data");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _hubConnection.StopAsync(cancellationToken);
            await _hubConnection.DisposeAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}