using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class PointService : IPointService
    {
        private readonly IPointRepository _pointRepository;

        public PointService(IPointRepository pointRepository)
        {
            _pointRepository = pointRepository;
        }

        public async Task<Point?> GetAsync(Guid pointId)
        {
            return await _pointRepository.GetAsync(pointId);
        }

        public async Task<Guid?> CreateAsync(Point point)
        {
            return await _pointRepository.CreateAsync(point);
        }

        public async Task<List<Point>?> GetMapPointsAsync(Guid mapId)
        {
            return await _pointRepository.GetMapPointsAsync(mapId);
        }

        public async Task<List<Point>?> GetConnectedPointsAsync(Guid pointId)
        {
            return await _pointRepository.GetConnectedPointsAsync(pointId);
        }
    }
}
