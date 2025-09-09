using Microsoft.AspNetCore.SignalR;
using Traffic.API.Hubs;

namespace Traffic.API.Services
{
    public class TransportCoordinatorService : BackgroundService
    {
        private readonly IHubContext<TransportAPIHub> _hubAPIContext;
        private readonly ILogger<TransportCoordinatorService> _logger;
        private readonly PeriodicTimer _requestTimer = new(TimeSpan.FromSeconds(2));

        public TransportCoordinatorService(IHubContext<TransportAPIHub> hubAPIContext,
            ILogger<TransportCoordinatorService> logger)
        {
            _hubAPIContext = hubAPIContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _requestTimer.WaitForNextTickAsync(stoppingToken))
            {
                await RequestData();
            }
        }

        private async Task RequestData()
        {
            try
            {
                await _hubAPIContext.Clients.Group("TrafficNow")
                    .SendAsync("TransportDataRequest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending transport data request");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _requestTimer?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}