using Dapper;
using System.Data;
using System.Text;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Entities;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class RouteRepository : RepositoryBase, IRouteRepository
    {
        private readonly IPointRepository _pointRepository;
        private readonly ITransportRepository _transportRepository;

        public RouteRepository(IDbConnection connection, IPointRepository pointRepository, ITransportRepository transportRepository)
            : base(connection)
        {
            _pointRepository = pointRepository;
            _transportRepository = transportRepository;
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

        public async Task<Guid?> CreateAsync(Route route)
        {
            const string sql = "SELECT create_route(@Id, @TransportId)";
            await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                route.Id,
                route.TransportId,
                route.RouteTime
            });

            return route.Id;
        }

        public async Task<Guid?> AddRoutePointAsync(Guid routeId, Guid pointId)
        {
            const string pointsSql = "SELECT create_route_points(@CurrentRouteId, @CurrentPointId)";
            await _connection.ExecuteScalarAsync<Guid>(pointsSql, new
            {
                CurrentRouteId = routeId,
                CurrentPointId = pointId
            });

            return routeId;
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

            var transportResult = await _transportRepository.GetAsync(result.TransportId);
            result.Transport = transportResult ?? default!;

            return result;
        }

        public async Task<List<Route>?> GetUserRoutesAsync(Guid userId)
        {
            const string sql = "SELECT * FROM select_user_routes(@CurrentUserId)";
            var routesResult = (await _connection.QueryAsync<RouteEntity>(sql, new
            {
                CurrentUserId = userId
            }));

            var result = new List<Route>(routesResult.Count());
            foreach (var route in routesResult)
            {
                var pointsResult = await _pointRepository.GetRoutePointsAsync(route.Id);

                result.Add(new Route(route));
                result.Last().EnqueuePoints(pointsResult ?? []);
            }

            return result;
        }
    }
}
