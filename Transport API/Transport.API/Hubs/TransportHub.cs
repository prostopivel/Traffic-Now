using Microsoft.AspNetCore.SignalR;

namespace Transport.API.Hubs
{
    public class TransportHub : Hub
    {
        public async Task SendDataToAll(string jsonData)
        {
            await Clients.All.SendAsync("TransportData", jsonData);
        }
    }
}
