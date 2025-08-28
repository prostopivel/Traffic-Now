using Dapper;
using System.Data;
using Traffic.Data.Entities;

namespace Traffic.Data.Repositories
{
    public class TransportRepository : RepositoryBase
    {
        public TransportRepository(IDbConnection connection) : base(connection)
        { }

        public async Task<TransportEntity?> GetAsync(Guid transportId)
        {
            const string sql = "SELECT * FROM select_transport(@TransportId)";
            var result = await _connection.QueryAsync<TransportEntity>(sql, new { TransportId = transportId });
            return result.FirstOrDefault();
        }

        public async Task<Guid?> CreateAsync(TransportEntity transport)
        {
            const string sql = "SELECT create_transport(@Id, @UserId, @PointId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                transport.Id,
                transport.UserId,
                transport.PointId
            });
        }

        public async Task<Guid?> UpdateAsync(TransportEntity transport)
        {
            const string sql = "SELECT update_transport(@Id, @UserId, @PointId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                transport.Id,
                transport.UserId,
                transport.PointId
            });
        }

        public async Task<Guid?> DeleteAsync(Guid transportId)
        {
            const string sql = "SELECT delete_transport(@TransportId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new { TransportId = transportId });
        }

        public async Task<List<TransportEntity>?> GetUserTransportAsync(Guid userId)
        {
            const string sql = "SELECT * FROM select_user_transport(@CurrentUserId)";
            var result = await _connection.QueryAsync<TransportEntity>(sql, new { CurrentUserId = userId });
            return [.. result];
        }

        public async Task<List<TransportEntity>?> GetUserTransportAsync(Guid mapId, Guid userId)
        {
            const string sql = "SELECT * FROM select_map_user_transport(@CurrentMapId, @CurrentUserId)";
            var result = await _connection.QueryAsync<TransportEntity>(sql, new { CurrentMapId = mapId, CurrentUserId = userId });
            return [.. result];
        }
    }
}
