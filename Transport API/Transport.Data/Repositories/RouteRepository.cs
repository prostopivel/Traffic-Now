using Transport.Core.Abstractions;
using Transport.Core.Models;

namespace Transport.Data.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly Queue<Point> _points = new Queue<Point>();
        private readonly Random _random = new Random();

        public int Speed { get; set; }

        public void GenerateRoute(Map map, Guid garageId)
        {
            _points.Clear();
            Speed = _random.Next(60, 80);

            GenerateNewRoute(map, garageId);
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
            return _points.ToList();
        }

        public bool HasNextPoint()
        {
            return _points.Count > 0;
        }

        public void ClearRoute()
        {
            _points.Clear();
        }

        private void GenerateNewRoute(Map map, Guid garageId)
        {
            var garagePoint = map.Points.FirstOrDefault(p => p.Id == garageId);
            if (garagePoint == null)
            {
                garagePoint = GetRandomPoint(map);
                if (garagePoint == null) return;
            }

            var route = GenerateDepthFirstRoute(map, garagePoint);

            int maxRouteLength = _random.Next(5, Math.Min(16, map.Points.Count + 1));
            var limitedRoute = route.Take(maxRouteLength).ToList();

            if (limitedRoute.Count > 0 && limitedRoute.Last().Id != garageId)
            {
                var returnPath = FindPathToGarage(map, limitedRoute.Last(), garagePoint);
                if (returnPath != null && returnPath.Count > 1)
                {
                    limitedRoute.AddRange(returnPath.Skip(1));
                }
                else
                {
                    limitedRoute.Add(garagePoint);
                }
            }

            foreach (var point in limitedRoute)
            {
                _points.Enqueue(point);
            }
        }

        private List<Point> GenerateDepthFirstRoute(Map map, Point startPoint)
        {
            var visited = new HashSet<Guid>();
            var route = new List<Point>();
            var stack = new Stack<Point>();

            stack.Push(startPoint);
            visited.Add(startPoint.Id);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                route.Add(current);

                var connectedPoints = GetConnectedPoints(map, current)
                    .Where(p => !visited.Contains(p.Id))
                    .ToList();

                if (connectedPoints.Count > 0)
                {
                    var shuffledPoints = connectedPoints.OrderBy(_ => _random.Next()).ToList();

                    foreach (var point in shuffledPoints)
                    {
                        if (!visited.Contains(point.Id))
                        {
                            stack.Push(point);
                            visited.Add(point.Id);
                        }
                    }
                }
            }

            return route;
        }

        private List<Point>? FindPathToGarage(Map map, Point startPoint, Point garagePoint)
        {
            var queue = new Queue<List<Point>>();
            var visited = new HashSet<Guid>();

            queue.Enqueue(new List<Point> { startPoint });
            visited.Add(startPoint.Id);

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path.Last();

                if (current.Id == garagePoint.Id)
                {
                    return path;
                }

                foreach (var connectedPoint in GetConnectedPoints(map, current))
                {
                    if (!visited.Contains(connectedPoint.Id))
                    {
                        visited.Add(connectedPoint.Id);
                        var newPath = new List<Point>(path) { connectedPoint };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return null;
        }

        private Point? GetRandomPoint(Map map)
        {
            if (map.Points.Count == 0) return null;
            return map.Points[_random.Next(map.Points.Count)];
        }

        private IEnumerable<Point> GetConnectedPoints(Map map, Point point)
        {
            foreach (var connectedPointId in point.ConnectedPointsIds)
            {
                var connectedPoint = map.Points.FirstOrDefault(p => p.Id == connectedPointId);
                if (connectedPoint != null)
                {
                    yield return connectedPoint;
                }
            }
        }
    }
}