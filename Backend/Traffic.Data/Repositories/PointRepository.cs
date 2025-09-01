using Dapper;
using System.Data;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Entities;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class PointRepository : RepositoryBase, IPointRepository
    {
        public PointRepository(IDbConnection connection) : base(connection)
        { }

        public async Task<Point?> GetAsync(Guid pointId)
        {
            const string sql = "SELECT * FROM select_point(@PointId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new
            {
                PointId = pointId
            });

            return new Point(result.FirstOrDefault());
        }

        public async Task<Guid?> CreateAsync(Point point)
        {
            const string sql = "SELECT create_point(@Id, @MapId, @X, @Y, @Name)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                point.Id,
                point.MapId,
                point.X,
                point.Y,
                point.Name
            });
        }

        public async Task<List<Point>?> GetMapPointsAsync(Guid mapId)
        {
            const string sql = "SELECT * FROM select_map_points(@CurrentMapId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new
            {
                CurrentMapId = mapId
            });

            return [.. await ConnectPoints(result)];
        }

        public async Task<List<Point>?> GetRoutePointsAsync(Guid routeId)
        {
            const string sql = "SELECT * FROM select_route_points(@CurrentRouteId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new
            {
                CurrentRouteId = routeId
            });

            return [.. await ConnectPoints(result)];
        }

        public async Task<List<Point>?> GetConnectedPointsAsync(Guid pointId)
        {
            const string sql = "SELECT * FROM select_connected_points(@PointId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new
            {
                PointId = pointId
            });

            return [.. result.Select(p => new Point(p))];
        }

        private async Task<IEnumerable<Point>> ConnectPoints(IEnumerable<PointEntity> pointEntities)
        {
            var points = new List<Point>();

            foreach (var p in pointEntities)
            {
                var point = new Point(p);
                var connectedPoints = await GetConnectedPointsAsync(point.Id);
                point.ConnectPoints(connectedPoints ?? []);
                points.Add(point);
            }

            return points;
        }
    }
}
