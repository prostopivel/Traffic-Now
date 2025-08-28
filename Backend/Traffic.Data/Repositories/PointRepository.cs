using Dapper;
using System.Data;
using Traffic.Data.Entities;

namespace Traffic.Data.Repositories
{
    public class PointRepository : RepositoryBase
    {
        public PointRepository(IDbConnection connection) : base(connection)
        { }

        public async Task<PointEntity?> GetAsync(Guid pointId)
        {
            const string sql = "SELECT * FROM select_point(@PointId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new { PointId = pointId });
            return result.FirstOrDefault();
        }

        public async Task<Guid?> CreateAsync(PointEntity point)
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

        public async Task<List<PointEntity>?> GetMapPointsAsync(Guid mapId)
        {
            const string sql = "SELECT * FROM select_map_points(@CurrentMapId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new { CurrentMapId = mapId });
            return [.. result];
        }

        public async Task<List<PointEntity>?> GetConnectedPointsAsync(Guid pointId)
        {
            const string sql = "SELECT * FROM select_connected_points(@PointId)";
            var result = await _connection.QueryAsync<PointEntity>(sql, new { PointId = pointId });
            return [.. result];
        }
    }
}
