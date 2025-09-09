using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Traffic.API.Hubs
{
    public class TransportClientHub : Hub
    {
        public async Task SubscribeToUser()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("SubscriptionError", "User not authenticated");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await Clients.Caller.SendAsync("SubscriptionConfirmed", userId);
        }

        public async Task UnsubscribeFromUser()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}