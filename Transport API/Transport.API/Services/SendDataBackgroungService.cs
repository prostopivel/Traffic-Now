using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Transport.API.Contracts;
using Transport.API.Hubs;
using Transport.Core.Abstractions;

namespace Transport.API.Services
{
    public class SendDataBackgroungService : BackgroundService
    {
        private readonly IHubContext<TransportHub> _hubContext;
        private readonly IDataService _dataService;
        private readonly ILogger<SendDataBackgroungService> _logger;

        public SendDataBackgroungService(IHubContext<TransportHub> hubContext,
            ILogger<SendDataBackgroungService> logger, IDataService dataService)
        {
            _hubContext = hubContext;
            _logger = logger;
            _dataService = dataService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_dataService.Transport == null)
                    {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    var data = new PointResponse(
                        _dataService.Transport.Id,
                        _dataService.Transport.X,
                        _dataService.Transport.Y);

                    string jsonData = JsonSerializer.Serialize(data);

                    await _hubContext.Clients.All.SendAsync("ReceiveData", jsonData, stoppingToken);

                    _logger.LogInformation("Data sent: {JsonData}", jsonData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending data");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
