using Transport.Core.Abstractions;
using Transport.Core.Models;
using System.Collections.Generic;
using System.Linq;

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

            var route = GenerateDepthFirstRoute(map, _garagePoint);

            int maxRouteLength = _random.Next(5, Math.Min(16, map.Points.Count + 1));
            var limitedRoute = route.Take(maxRouteLength).ToList();

            foreach (var point in limitedRoute)
            {
                _points.Enqueue(point);
            }
        }

        private List<Point> GenerateDepthFirstRoute(Map map, Point startPoint)
        {
            var route = new List<Point>();
            var visited = new HashSet<Guid>();
            var stack = new Stack<Point>();

            stack.Push(startPoint);
            visited.Add(startPoint.Id);

            while (stack.Count > 0 && route.Count < map.Points.Count * 2)
            {
                var current = stack.Pop();
                route.Add(current);

                var connectedPoints = GetConnectedPoints(map, current).ToList();

                if (connectedPoints.Count > 0)
                {
                    var unvisitedPoints = connectedPoints.Where(p => !visited.Contains(p.Id)).ToList();
                    var visitedPoints = connectedPoints.Where(p => visited.Contains(p.Id)).ToList();

                    var shuffledUnvisited = unvisitedPoints.OrderBy(_ => _random.Next()).ToList();
                    foreach (var point in shuffledUnvisited)
                    {
                        visited.Add(point.Id);
                        stack.Push(point);
                    }

                    if (visitedPoints.Count > 0 && _random.NextDouble() < 0.3)
                    {
                        var randomVisitedPoint = visitedPoints[_random.Next(visitedPoints.Count)];
                        stack.Push(randomVisitedPoint);
                    }
                }
            }

            return route;
        }

        private Point? GetRandomPoint(Map map)
        {
            if (map.Points.Count == 0) return null;
            return map.Points[_random.Next(map.Points.Count)];
        }

        private static IEnumerable<Point> GetConnectedPoints(Map map, Point point)
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