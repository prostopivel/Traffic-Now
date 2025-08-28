using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Repositories
{
    public interface IMapRepository
    {
        Task<Map?> GetAsync(Guid mapId);
        Task<Guid?> CreateAsync(Map map);
        Task<Guid?> UpdateAsync(Map map);
        Task<Guid?> DeleteAsync(Guid mapId);
    }
}