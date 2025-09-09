using System.Text.Json;
using System.Text.Json.Serialization;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;

namespace Traffic.Application.Services
{
    public class MapSerializeService : IMapSerializeService
    {
        private readonly IMapRepository _mapRepository;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public MapSerializeService(IMapRepository mapRepository)
        {
            _mapRepository = mapRepository;
        }

        public async Task<Guid> CreateMapJson(string path, Guid mapId)
        {
            var json = await ExportMapJson(mapId);
            File.WriteAllText(path, json);

            return mapId;
        }

        public async Task<string> ExportMapJson(Guid mapId)
        {
            var map = await _mapRepository.GetMapPointsAsync(mapId);

            string json = JsonSerializer.Serialize(map, _jsonSerializerOptions);

            return json;
        }
    }
}