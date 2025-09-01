using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface IMapService
    {
        Task<Guid?> CreateAsync(Map map);
        Task<Guid?> DeleteAsync(Guid mapId);
        Task<Map?> GetAsync(Guid mapId);
        Task<Guid?> UpdateAsync(Map map);
        Task<Map?> GetMapPointsAsync(Guid mapId);
        Task<List<Map>?> GetUserMaps(Guid userId);
        Task<Guid?> AddUserMap(Guid userId, Guid mapId);
        Task<(Guid?, string Error)> CreateMapPointsAsync(IEnumerable<Point> points);
    }
}