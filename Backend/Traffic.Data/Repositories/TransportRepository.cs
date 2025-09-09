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

            var result = new Transport(transportResult.FirstOrDefault());
            var pointResult = await _pointRepository.GetAsync(result?.PointId ?? Guid.Empty);
            var users = await GetTransportUsersAsync(transportId);

            if (result == null || pointResult == null || users == null)
            {
                return null;
            }

            result.Point = pointResult;
            result.UsersId = users;

            return result;
        }

        public async Task<Guid?> CreateAsync(Transport transport)
        {
            var newTransport = await GetAsync(transport.Id);
            if (newTransport != null && newTransport.Id != Guid.Empty)
            {
                var id = await AddTransportUserAsync(transport.Id, transport.UsersId.Last());
                return id;
            }

            const string sql = "SELECT create_transport(@Id, @PointId, @Url, @UserId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                transport.Id,
                transport.PointId,
                transport.Url,
                UserId = transport.UsersId.Last()
            });
        }

        public async Task<Guid?> UpdateAsync(Transport transport)
        {
            const string sql = "SELECT update_transport(@Id, @UserId, @PointId, @Url)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                transport.Id,
                transport.PointId,
                transport.Url,
                UserId = transport.UsersId.Last()
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

        public async Task<List<(Guid, string)>?> GetUrlsAsync()
        {
            const string sql = "SELECT * FROM select_transport_url()";
            return [.. await _connection.QueryAsync<(Guid, string)>(sql)];
        }

        public async Task<Guid?> DeleteTransportUserAsync(Guid transportId, Guid userId)
        {
            const string sql = "SELECT delete_transport_user(@TransportId, @UserId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                transportId,
                userId
            });
        }

        public async Task<List<Transport>?> GetUserTransportAsync(Guid userId)
        {
            const string sql = "SELECT * FROM select_user_transport(@CurrentUserId)";
            var transportEntities = await _connection.QueryAsync<TransportEntity>(sql, new
            {
                CurrentUserId = userId
            });

            var result = new List<Transport>(transportEntities.Count());
            foreach (var item in transportEntities)
            {
                result.Add(new Transport(item));
                var pointResult = await _pointRepository.GetAsync(item?.PointId ?? Guid.Empty);
                var users = await GetTransportUsersAsync(item?.Id ?? Guid.Empty);

                if (pointResult == null || users == null)
                {
                    return null;
                }

                result.Last().Point = pointResult;
                result.Last().UsersId = users;
            }

            return result;
        }

        public async Task<List<Transport>?> GetUserTransportAsync(Guid mapId, Guid userId)
        {
            const string sql = "SELECT * FROM select_map_user_transport(@CurrentMapId, @CurrentUserId)";
            var transportEntities = await _connection.QueryAsync<TransportEntity>(sql, new
            {
                CurrentMapId = mapId,
                CurrentUserId = userId
            });

            var result = new List<Transport>(transportEntities.Count());
            foreach (var item in transportEntities)
            {
                result.Add(new Transport(item));
                var pointResult = await _pointRepository.GetAsync(item?.PointId ?? Guid.Empty);
                var users = await GetTransportUsersAsync(item?.Id ?? Guid.Empty);

                if (pointResult == null || users == null)
                {
                    return null;
                }

                result.Last().Point = pointResult;
                result.Last().UsersId = users;
            }

            return result;
        }

        public async Task<List<Guid>> GetTransportUsersAsync(Guid transportId)
        {
            const string sql = "SELECT * FROM select_transport_users(@TransportId)";
            var transportGuids = await _connection.QueryAsync<Guid>(sql, new
            {
                TransportId = transportId
            });

            return [.. transportGuids];
        }

        private async Task<Guid> AddTransportUserAsync(Guid transportId, Guid userId)
        {
            const string sql = "SELECT add_transport_user(@TransportId, @UserId)";
            var result = await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                TransportId = transportId,
                UserId = userId
            });

            return result;
        }
    }
}
