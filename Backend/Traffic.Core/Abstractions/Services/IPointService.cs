using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface IPointService
    {
        Task<Guid?> CreateAsync(Point point);
        Task<Point?> GetAsync(Guid pointId);
        Task<List<Point>?> GetConnectedPointsAsync(Guid pointId);
        Task<List<Point>?> GetMapPointsAsync(Guid mapId);
    }
}