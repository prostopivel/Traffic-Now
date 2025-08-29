using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class TransportService : ITransportService
    {
        private readonly ITransportRepository _transportRepository;

        public TransportService(ITransportRepository transportRepository)
        {
            _transportRepository = transportRepository;
        }

        public async Task<Transport?> GetAsync(Guid transportId)
        {
            return await _transportRepository.GetAsync(transportId);
        }

        public async Task<Guid?> CreateAsync(Transport transport)
        {
            return await _transportRepository.CreateAsync(transport);
        }

        public async Task<Guid?> UpdateAsync(Transport transport)
        {
            return await _transportRepository.UpdateAsync(transport);
        }

        public async Task<Guid?> DeleteAsync(Guid transportId)
        {
            return await _transportRepository.DeleteAsync(transportId);
        }

        public async Task<List<Transport>?> GetUserTransportAsync(Guid userId)
        {
            return await _transportRepository.GetUserTransportAsync(userId);
        }

        public async Task<List<Transport>?> GetUserTransportAsync(Guid mapId, Guid userId)
        {
            return await _transportRepository.GetUserTransportAsync(mapId, userId);
        }
    }
}
