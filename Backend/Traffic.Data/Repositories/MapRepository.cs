using Dapper;
using System.Data;
using Traffic.Data.Entities;

namespace Traffic.Data.Repositories
{
    public class MapRepository
    {
        private readonly IDbConnection _connection;

        public MapRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<MapEntity?> GetAsync(Guid mapId)
        {
            const string sql = "SELECT * FROM select_map(@MapId)";
            var result = await _connection.QueryAsync<MapEntity>(sql, new { MapId = mapId });
            return result.FirstOrDefault();
        }

        public async Task<Guid?> CreateAsync(MapEntity map)
        {
            const string sql = "SELECT create_map(@Id, @Name)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new { Map = map });
        }

        public async Task<MapEntity?> UpdateAsync(MapEntity map)
        {
            const string sql = "SELECT update_map(@Id, @Name)";
            return await _connection.ExecuteScalarAsync<MapEntity>(sql, new { Map = map });
        }

        public async Task<MapEntity?> DeleteAsync(Guid mapId)
        {
            const string sql = "SELECT delete_map(@MapId)";
            return await _connection.ExecuteScalarAsync<MapEntity>(sql, new { MapId = mapId });
        }
    }
}
