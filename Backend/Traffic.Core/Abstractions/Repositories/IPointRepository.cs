using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Repositories
{
    public interface IPointRepository
    {
        Task<Point?> GetAsync(Guid pointId);
        Task<Guid?> CreateAsync(Point point);
        Task<List<Point>?> GetMapPointsAsync(Guid mapId);
        Task<List<Point>?> GetRoutePointsAsync(Guid routeId);
        Task<List<Point>?> GetConnectedPointsAsync(Guid pointId);
    }
}