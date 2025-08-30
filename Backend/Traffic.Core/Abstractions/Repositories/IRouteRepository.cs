using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public interface IRouteRepository
    {
        Task<Route?> GetAsync(Guid routeId);
        Task<Guid?> CreateAsync(Route route, IEnumerable<Point> points);
        Task<Route?> GetRoutePointsAsync(Guid routeId);
        Task<List<Route>?> GetUserRoutesAsync(Guid userId);
    }
}