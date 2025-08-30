using Dapper;
using System.Data;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Entities;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class RouteRepository : RepositoryBase, IRouteRepository
    {
        private readonly IPointRepository _pointRepository;

        public RouteRepository(IDbConnection connection, IPointRepository pointRepository)
            : base(connection)
        {
            _pointRepository = pointRepository;
        }

        public async Task<Route?> GetAsync(Guid routeId)
        {
            const string sql = "SELECT * FROM select_route(@RouteId)";
            var result = await _connection.QueryAsync<RouteEntity>(sql, new
            {
                RouteId = routeId
            });

            return new Route(result.FirstOrDefault());
        }

        public async Task<Guid?> CreateAsync(Route route, IEnumerable<Point> points)
        {
            const string sql = "SELECT create_route(@Id, @TransportId)";
            await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                route.Id,
                route.TransportId,
                route.RouteTime
            });

            const string pointsSql = "SELECT create_route_points(@CurrentRouteId, @CurrentPointId)";
            foreach (var point in points)
            {
                await _connection.ExecuteScalarAsync<Guid>(pointsSql, new
                {
                    CurrentRouteId = route.Id,
                    CurrentPointId = point.Id
                });
            }

            return route.Id;
        }

        public async Task<Route?> GetRoutePointsAsync(Guid routeId)
        {
            const string sql = "SELECT * FROM select_route(@CurrentRouteId)";
            var routeResult = await _connection.QueryAsync<RouteEntity>(sql, new
            {
                CurrentRouteId = routeId
            });

            var pointsResult = await _pointRepository.GetRoutePointsAsync(routeId);

            var result = new Route(routeResult.FirstOrDefault());
            result.EnqueuePoints(pointsResult ?? []);

            return result;
        }

        public async Task<List<Route>?> GetUserRoutesAsync(Guid userId)
        {
            const string sql = "SELECT * FROM select_user_routes(@CurrentUserId)";
            var result = (await _connection.QueryAsync<RouteEntity>(sql, new
            {
                CurrentUserId = userId
            }));

            return [.. result.Select(r => new Route(r))];
        }
    }
}
