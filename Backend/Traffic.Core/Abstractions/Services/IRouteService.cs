using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface IRouteService
    {
        Task<Route?> GetAsync(Guid routeId);
        Task<Guid?> CreateAsync(Route route);
        Task<Guid?> AddRoutePointAsync(Guid routeId, Guid pointId);
        Task<Route?> GetRoutePointsAsync(Guid routeId);
        Task<List<Route>?> GetUserRoutesAsync(Guid userId);
    }
}