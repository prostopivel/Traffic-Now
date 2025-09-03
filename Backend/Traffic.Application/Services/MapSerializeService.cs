using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Entities;

namespace Traffic.Application.Services
{
    public class MapSerializeService : IMapSerializeService
    {
        private readonly IMapRepository _mapRepository;

        public MapSerializeService(IMapRepository mapRepository)
        {
            _mapRepository = mapRepository;
        }

        public async Task<Guid> CreateMapJson(string path, Guid mapId)
        {
            var map = await _mapRepository.GetMapPointsAsync(mapId);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(map, options);
            File.WriteAllText(path, json);

            return mapId;
        }
    }
}
