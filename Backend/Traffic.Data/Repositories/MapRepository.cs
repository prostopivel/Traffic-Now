using Dapper;
using System.Data;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Entities;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class MapRepository : RepositoryBase, IMapRepository
    {
        private readonly IPointRepository _pointRepository;

        public MapRepository(IDbConnection connection, IPointRepository pointRepository)
            : base(connection)
        {
            _pointRepository = pointRepository;
        }

        public async Task<Map?> GetAsync(Guid mapId)
        {
            const string sql = "SELECT * FROM select_map(@MapId)";
            var result = await _connection.QueryAsync<MapEntity>(sql, new
            {
                MapId = mapId
            });

            return new Map(result.FirstOrDefault());
        }

        public async Task<Guid?> CreateAsync(Map map)
        {
            const string sql = "SELECT create_map(@Id, @Name)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                map.Id,
                map.Name
            });
        }

        public async Task<Guid?> UpdateAsync(Map map)
        {
            const string sql = "SELECT update_map(@Id, @Name)";

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                map.Id,
                map.Name
            });
        }

        public async Task<Guid?> DeleteAsync(Guid mapId)
        {
            const string sql = "SELECT delete_map(@MapId)";

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                MapId = mapId
            });
        }

        public async Task<Map?> GetMapPointsAsync(Guid mapId)
        {
            var result = await GetAsync(mapId);
            var pointsResult = await _pointRepository.GetMapPointsAsync(mapId);

            pointsResult?.ForEach(p => result?.AddPoint(p));

            return result;
        }

        public async Task<List<Map>?> GetUserMaps(Guid userId)
        {
            const string sql = "SELECT * FROM select_user_maps(@UserId)";

            var maps = await _connection.QueryAsync<MapEntity>(sql, new
            {
                UserId = userId
            });

            return [.. maps.Select(m => new Map(m))];
        }

        public async Task<Guid?> AddUserMap(Guid userId, Guid mapId)
        {
            const string sql = "SELECT add_user_map(@UserId, @MapId)";

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                UserId = userId,
                MapId = mapId
            });
        }

        public async Task<(Guid?, string Error)> CreateMapPointsAsync(IEnumerable<Point> points)
        {
            var Error = string.Empty;
            if (!points.Any())
            {
                Error = $"Точек нет!";
                return (null, Error);
            }

            var mapId = points?.First().MapId;
            if (mapId == null || await GetAsync((Guid)mapId) == null)
            {
                Error = $"Карта с Id '{mapId}' не существует!";
                return (null, Error);
            }

            using var transaction = _connection.BeginTransaction();
            try
            {
                //Создание точек
                foreach (var point in points!)
                {
                    await _pointRepository.CreateAsync(point);
                }

                //Соединение точек
                const string connectSql = "SELECT connect_points(@PointLeftId, @PointRightId)";
                foreach (var point in points)
                {
                    if (point == null)
                        continue;
                    foreach (var connectedPoint in point.ConnectedPoints ?? [])
                    {
                        await _connection.ExecuteScalarAsync<Guid>(connectSql, new
                        {
                            PointLeftId = point?.Id,
                            PointRightId = connectedPoint.Id
                        });
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Error = $"Ошибка при создании точек: {ex.Message}";
            }

            return (mapId, Error);
        }
    }
}
