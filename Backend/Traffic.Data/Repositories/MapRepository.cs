using Dapper;
using System.Data;
using Traffic.Core.Entities;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class MapRepository : RepositoryBase, IMapRepository
    {
        public MapRepository(IDbConnection connection) : base(connection)
        { }

        public async Task<Map?> GetAsync(Guid mapId)
        {
            const string sql = "SELECT * FROM select_map(@MapId)";
            var result = await _connection.QueryAsync<MapEntity>(sql, new { MapId = mapId });
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
            return await _connection.ExecuteScalarAsync<Guid>(sql, new { MapId = mapId });
        }
    }
}
