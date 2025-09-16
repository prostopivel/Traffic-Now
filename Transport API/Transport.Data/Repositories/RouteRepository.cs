using Transport.Core.Abstractions;
using Transport.Core.Algorithms;
using Transport.Core.Models;

namespace Transport.Data.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly Queue<Point> _points = new();
        private readonly Random _random = new();
        private Point? _garagePoint;

        public int Speed { get; set; }

        public void GenerateRoute(Map map, Guid garageId)
        {
            _points.Clear();
            Speed = _random.Next(60, 80);
            _garagePoint = map.Points.FirstOrDefault(p => p.Id == garageId);

            GenerateNewRoute(map);
        }

        public Point? GetPoint()
        {
            return _points.Count > 0 ? _points.Dequeue() : null;
        }

        public Point? PeekPoint()
        {
            return _points.Count > 0 ? _points.Peek() : null;
        }

        public List<Point> GetCurrentRoute()
        {
            return [.. _points];
        }

        public bool HasNextPoint()
        {
            return _points.Count > 0;
        }

        public void ClearRoute()
        {
            _points.Clear();
            _garagePoint = null;
        }

        private void GenerateNewRoute(Map map)
        {
            if (_garagePoint == null)
            {
                _garagePoint = GetRandomPoint(map);
                if (_garagePoint == null) return;
            }

            Point? targetPoint;
            do
            {
                targetPoint = GetRandomPoint(map);
            } while (targetPoint == null || targetPoint.Id == _garagePoint.Id);

            var optimalPath = DijkstraAlgorithm.FindShortestPath(map, _garagePoint, targetPoint);

            if (optimalPath != null && optimalPath.Count > 0)
            {
                foreach (var point in optimalPath)
                {
                    _points.Enqueue(point);
                }
            }
        }

        private Point? GetRandomPoint(Map map)
        {
            if (map.Points.Count == 0)
            {
                return null;
            }

            return map.Points[_random.Next(map.Points.Count)];
        }
    }
}