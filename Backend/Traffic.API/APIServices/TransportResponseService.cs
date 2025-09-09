using Microsoft.AspNetCore.SignalR;
using Traffic.API.Contracts;
using Traffic.API.Hubs;
using Traffic.Application.Services;

namespace Traffic.API.APIServices
{
    public class TransportResponseService : BackgroundService
    {
        private readonly IHubContext<TransportClientHub> _hubClientContext;
        private readonly ITransportDataService _transportDataService;
        private readonly ILogger<TransportResponseService> _logger;
        private readonly PeriodicTimer _responseTimer = new(TimeSpan.FromSeconds(2));

        public TransportResponseService(IHubContext<TransportClientHub> hubClientContext,
            ITransportDataService transportDataService, ILogger<TransportResponseService> logger)
        {
            _hubClientContext = hubClientContext;
            _transportDataService = transportDataService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _responseTimer.WaitForNextTickAsync(stoppingToken))
            {
                await ResponseData();
            }
        }

        private async Task ResponseData()
        {
            try
            {
                int userCount = 0;

                foreach (var userEntry in _transportDataService.UserTransport)
                {
                    var userId = userEntry.Key;
                    var transportList = userEntry.Value;

                    var transportDataList = _transportDataService[userId];

                    if (transportDataList?.Count > 0)
                    {
                        try
                        {
                            var responseList = transportDataList
                                .Select(kvp => new PositionTransportResponse(
                                    kvp.Key,
                                    kvp.Value.X,
                                    kvp.Value.Y))
                                .ToList();

                            await _hubClientContext.Clients.Group(userId.ToString())
                                .SendAsync("TransportMapData", responseList);

                            //int i = 0;
                            //foreach (var item in responseList)
                            //{
                            //    Console.WriteLine($"{i} | {item.TransportId} - {item?.X}:{item?.Y}");
                            //    i++;
                            //}
                            userCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to send data to user {UserId}", userId);
                        }
                    }
                }

                _logger.LogDebug("Sent transport data to {UserCount} users", userCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending transport data to users");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _responseTimer?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}
