using Traffic.Core.Abstractions.Services;

namespace Traffic.Application.Services
{
    public class ConnectionTransportService : IConnectionTransportService
    {
        private readonly ITransportAPIConnection _transportAPIConnection;
        private readonly ITransportService _transportService;

        public ConnectionTransportService(ITransportAPIConnection transportAPIConnection,
            ITransportService transportService)
        {
            _transportAPIConnection = transportAPIConnection;
            _transportService = transportService;
        }

        public async Task<bool> ConnectUserTransport(Guid userId)
        {
            var userTransport = await _transportService.GetUserTransportAsync(userId);

            foreach (var transport in userTransport ?? [])
            {
                await _transportAPIConnection.AddConnectionAsync(transport.Id.ToString(), transport.Url);
            }

            return true;
        }

        public async Task<bool> DisconnectUserTransport(Guid userId)
        {
            var userTransport = await _transportService.GetUserTransportAsync(userId);

            foreach (var transport in userTransport ?? [])
            {
                await _transportAPIConnection.RemoveConnectionAsync(transport.Id.ToString());
            }

            return true;
        }
    }
}
