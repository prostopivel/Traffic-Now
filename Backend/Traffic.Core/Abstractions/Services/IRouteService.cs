using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface IRouteService
    {
        Task<Guid?> CreateAsync(Route route, IEnumerable<Point> points);
        Task<Route?> GetAsync(Guid routeId);
        Task<Route?> GetRoutePointsAsync(Guid routeId);
        Task<List<Route>?> GetUserRoutesAsync(Guid userId);
    }
}