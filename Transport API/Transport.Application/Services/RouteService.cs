using Transport.Core.Abstractions;
using Transport.Core.Models;

namespace Transport.Application.Services
{
    public class RouteService : IRouteService
    {
        private readonly IDataService _dataService;
        private readonly IRouteRepository _routeRepository;

        public RouteService(IDataService dataService, IRouteRepository routeRepository)
        {
            _dataService = dataService;
            _routeRepository = routeRepository;
        }

        public int Speed
        {
            get => _routeRepository.Speed;
            set => _routeRepository.Speed = value;
        }

        public void GenerateRoute()
        {
            var map = _dataService.Map;
            Guid garageId = _dataService.Transport.PointId;
            _routeRepository.GenerateRoute(map, garageId);

            var random = new Random();
            Speed = random.Next(40, 60);
        }

        public List<Point> GetCurrentRoute()
        {
            return _routeRepository.GetCurrentRoute();
        }

        public Point? GetPoint()
        {
            return _routeRepository.GetPoint();
        }

        public Point? PeekPoint()
        {
            return _routeRepository.PeekPoint();
        }

        public bool HasNextPoint()
        {
            return _routeRepository.HasNextPoint();
        }

        public void ClearRoute()
        {
            _routeRepository.ClearRoute();
        }
    }
}
