using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class MapService : IMapService
    {
        private readonly IMapRepository _mapRepository;

        public MapService(IMapRepository mapRepository)
        {
            _mapRepository = mapRepository;
        }

        public async Task<Map?> GetAsync(Guid mapId)
        {
            return await _mapRepository.GetAsync(mapId);
        }

        public async Task<Guid?> CreateAsync(Map map)
        {
            return await _mapRepository.CreateAsync(map);
        }

        public async Task<Guid?> UpdateAsync(Map map)
        {
            return await _mapRepository.UpdateAsync(map);
        }

        public async Task<Guid?> DeleteAsync(Guid mapId)
        {
            return await _mapRepository.DeleteAsync(mapId);
        }
    }
}
