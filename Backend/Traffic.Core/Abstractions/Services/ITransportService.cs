using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface ITransportService
    {
        Task<Guid?> CreateAsync(Transport transport);
        Task<Transport?> GetAsync(Guid transportId);
        Task<Guid?> UpdateAsync(Transport transport);
        Task<Guid?> DeleteAsync(Guid transportId);
        Task<List<Transport>?> GetUserTransportAsync(Guid userId);
        Task<List<Transport>?> GetUserTransportAsync(Guid mapId, Guid userId);
    }
}