using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Repositories
{
    public interface IMapRepository
    {
        Task<Map?> GetAsync(Guid mapId);
        Task<Guid?> CreateAsync(Map map, IEnumerable<Point> points);
        Task<Guid?> UpdateAsync(Map map);
        Task<Guid?> DeleteAsync(Guid mapId);
        Task<Map?> GetMapPointsAsync(Guid mapId);
        Task<List<Map>?> GetUserMap(Guid userId);
        Task<Guid?> AddUserMap(Guid userId, Guid mapId);
        Task<(Guid?, string Error)> CreateMapPointsAsync(IEnumerable<Point> points);
    }
}