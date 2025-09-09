using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Traffic.API.Contracts;
using Traffic.Application.Services;

namespace Traffic.API.Hubs
{
    public class TransportAPIHub : Hub
    {
        private readonly ILogger<TransportAPIHub> _logger;
        private readonly ITransportDataService _transportDataService;

        public TransportAPIHub(ILogger<TransportAPIHub> logger,
            ITransportDataService transportDataService)
        {
            _logger = logger;
            _transportDataService = transportDataService;
        }

        public async Task SubscribeAsTransportServer(string serverId = "")
        {
            if (serverId == string.Empty)
            {
                serverId = Guid.NewGuid().ToString();
            }

            _logger.LogInformation("Server {ServerId} connected with connection {ConnectionId}",
                serverId, Context.ConnectionId);

            Context.Items["ServerId"] = serverId;
            await Groups.AddToGroupAsync(Context.ConnectionId, "TrafficNow");
            await Clients.Caller.SendAsync("SubscriptionConfirmed", "TrafficNow");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var serverId = Context.Items["ServerId"] as string;
            _logger.LogInformation("Server {ServerId} disconnected: {ConnectionId}",
                serverId, Context.ConnectionId);

            if (!string.IsNullOrEmpty(serverId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "TrafficNow");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}