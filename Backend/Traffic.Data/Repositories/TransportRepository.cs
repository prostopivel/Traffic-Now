using Dapper;
using System.Data;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Entities;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class TransportRepository : RepositoryBase, ITransportRepository
    {
        private readonly IPointRepository _pointRepository;

        public TransportRepository(IDbConnection connection, IPointRepository pointRepository) : base(connection)
        {
            _pointRepository = pointRepository;
        }

        public async Task<Transport?> GetAsync(Guid transportId)
        {
            const string sql = "SELECT * FROM select_transport(@TransportId)";
            var transportResult = await _connection.QueryAsync<TransportEntity>(sql, new
            {
                TransportId = transportId
            });

            var result = transportResult.FirstOrDefault();
            var pointResult = await _pointRepository.GetAsync(result?.PointId ?? Guid.Empty);

            return new Transport();
        }

        public async Task<Guid?> CreateAsync(Transport transport)
        {
            const string sql = "SELECT create_transport(@Id, @UserId, @PointId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                transport.Id,
                transport.UserId,
                transport.PointId
            });
        }

        public async Task<Guid?> UpdateAsync(Transport transport)
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

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                TransportId = transportId
            });
        }

        public async Task<List<Transport>?> GetUserTransportAsync(Guid userId)
        {
            const string sql = "SELECT * FROM select_user_transport(@CurrentUserId)";
            var result = await _connection.QueryAsync<TransportEntity>(sql, new
            {
                CurrentUserId = userId
            });

            return [.. result.Select(t => new Transport(t))];
        }

        public async Task<List<Transport>?> GetUserTransportAsync(Guid mapId, Guid userId)
        {
            const string sql = "SELECT * FROM select_map_user_transport(@CurrentMapId, @CurrentUserId)";
            var result = await _connection.QueryAsync<TransportEntity>(sql, new
            {
                CurrentMapId = mapId,
                CurrentUserId = userId
            });

            return [.. result.Select(t => new Transport(t))];
        }
    }
}
