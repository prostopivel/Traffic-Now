using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface IMapService
    {
        Task<Guid?> CreateAsync(Map map);
        Task<Map?> GetAsync(Guid mapId);
        Task<Guid?> DeleteAsync(Guid mapId);
        Task<Guid?> UpdateAsync(Map map);
        Task<Map?> GetMapPointsAsync(Guid mapId);
        Task<(Guid?, string Error)> CreateMapPointsAsync(IEnumerable<Point> points);
    }
}