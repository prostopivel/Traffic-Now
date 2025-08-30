using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;

        public RouteService(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<Route?> GetAsync(Guid routeId)
        {
            return await _routeRepository.GetAsync(routeId);
        }

        public async Task<Guid?> CreateAsync(Route route, IEnumerable<Point> points)
        {
            return await _routeRepository.CreateAsync(route, points);
        }

        public async Task<Route?> GetRoutePointsAsync(Guid routeId)
        {
            return await _routeRepository.GetRoutePointsAsync(routeId);
        }

        public async Task<List<Route>?> GetUserRoutesAsync(Guid userId)
        {
            return await _routeRepository.GetUserRoutesAsync(userId);
        }
    }
}
