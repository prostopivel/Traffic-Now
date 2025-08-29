using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface IMapService
    {
        Task<Guid?> CreateAsync(Map map);
        Task<Guid?> DeleteAsync(Guid mapId);
        Task<Map?> GetAsync(Guid mapId);
        Task<Guid?> UpdateAsync(Map map);
    }
}