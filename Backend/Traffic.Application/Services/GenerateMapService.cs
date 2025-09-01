using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class GenerateMapService
    {
        private const int MIN_POINT_COUNT = 100, MAX_POINT_COUNT = 150,
            MIN_DISTANCE = 6, MAX_DISTANCE = 15, PROBABILITY_CONNECT_POINT = 30;

        public static Task<(List<Point>, string Error)> GenerateMap(Map map)
        {
            return Task.Run(() =>
            {
                var rnd = new Random();
                var points = new List<Point>(rnd.Next(MIN_POINT_COUNT, MAX_POINT_COUNT));

                for (int i = 0; i < points.Capacity; i++)
                {
                    (var point, var Error) = Point.Create(
                        Guid.NewGuid(),
                        map.Id,
                        rnd.NextDouble() * 100,
                        rnd.NextDouble() * 100);

                    if (!string.IsNullOrEmpty(Error) || point == null)
                    {
                        return (points, Error);
                    }

                    point.Name = $"{(int)point.X}:{(int)point.Y}";
                    points.Add(point);
                }

                for (int i = 0; i < points.Count; i++)
                {
                    var nearPoints = points
                        .Where(p => CalculateDistance(points[i], p) < MAX_DISTANCE
                            && points[i].Id != p.Id);

                    foreach (var point in nearPoints)
                    {
                        if (rnd.Next(0, 100) > PROBABILITY_CONNECT_POINT)
                            continue;

                        points[i].ConnectPoint(point);
                    }
                }

                points = FindLargestConnectedComponent(points);
                var result = RemoveClosePointsAndCleanConnections(points, MIN_DISTANCE);
                result = RemoveIntersectingConnections(result);
                result = FindLargestConnectedComponent(result);

                return (result, string.Empty);
            });
        }

        private static double CalculateDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private static List<Point> RemoveClosePointsAndCleanConnections(List<Point> points, double minDistance)
        {
            var result = new List<Point>();
            var pointsToRemove = new HashSet<Guid>();

            for (int i = 0; i < points.Count; i++)
            {
                if (pointsToRemove.Contains(points[i].Id))
                    continue;

                for (int j = i + 1; j < points.Count; j++)
                {
                    if (pointsToRemove.Contains(points[j].Id))
                        continue;

                    double distance = CalculateDistance(points[i], points[j]);
                    if (distance < minDistance)
                    {
                        if (points[i].ConnectedPoints.Count <= points[j].ConnectedPoints.Count)
                        {
                            pointsToRemove.Add(points[i].Id);
                            break;
                        }
                        else
                        {
                            pointsToRemove.Add(points[j].Id);
                        }
                    }
                }
            }

            foreach (var point in points)
            {
                var connectionsToRemove = point.ConnectedPoints
                    .Where(p => pointsToRemove.Contains(p.Id))
                    .ToList();

                foreach (var connection in connectionsToRemove)
                {
                    point.DisconnectPoint(connection.Id);
                }
            }

            result.AddRange(points.Where(p => !pointsToRemove.Contains(p.Id)));

            return result;
        }

        private static List<Point> RemoveIntersectingConnections(List<Point> points)
        {
            var connectionsToRemove = new HashSet<(Guid, Guid)>();
            var allConnections = GetAllConnections(points);

            for (int i = 0; i < allConnections.Count; i++)
            {
                var conn1 = allConnections[i];

                for (int j = i + 1; j < allConnections.Count; j++)
                {
                    var conn2 = allConnections[j];

                    if (conn1.p1.Id == conn2.p1.Id || conn1.p1.Id == conn2.p2.Id ||
                        conn1.p2.Id == conn2.p1.Id || conn1.p2.Id == conn2.p2.Id)
                    {
                        continue;
                    }

                    if (DoLinesIntersect(conn1.p1, conn1.p2, conn2.p1, conn2.p2))
                    {
                        double length1 = CalculateDistance(conn1.p1, conn1.p2);
                        double length2 = CalculateDistance(conn2.p1, conn2.p2);

                        if (length1 >= length2)
                        {
                            connectionsToRemove.Add((conn1.p1.Id, conn1.p2.Id));
                        }
                        else
                        {
                            connectionsToRemove.Add((conn2.p1.Id, conn2.p2.Id));
                        }
                    }
                }
            }

            foreach (var point in points)
            {
                var connections = point.ConnectedPoints.ToList();
                foreach (var connectedPoint in connections)
                {
                    var connectionKey1 = (point.Id, connectedPoint.Id);
                    var connectionKey2 = (connectedPoint.Id, point.Id);

                    if (connectionsToRemove.Contains(connectionKey1) ||
                        connectionsToRemove.Contains(connectionKey2))
                    {
                        point.DisconnectPoint(connectedPoint.Id);
                    }
                }
            }

            return points;
        }

        private static List<(Point p1, Point p2)> GetAllConnections(List<Point> points)
        {
            var connections = new List<(Point, Point)>();
            var addedConnections = new HashSet<(Guid, Guid)>();

            foreach (var point in points)
            {
                foreach (var connectedPoint in point.ConnectedPoints)
                {
                    var key1 = (point.Id, connectedPoint.Id);
                    var key2 = (connectedPoint.Id, point.Id);

                    if (!addedConnections.Contains(key1) && !addedConnections.Contains(key2))
                    {
                        connections.Add((point, connectedPoint));
                        addedConnections.Add(key1);
                    }
                }
            }

            return connections;
        }

        private static bool DoLinesIntersect(Point a1, Point a2, Point b1, Point b2)
        {
            double Orientation(Point p, Point q, Point r)
            {
                return (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            }

            bool OnSegment(Point p, Point q, Point r)
            {
                return q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                       q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
            }

            double o1 = Orientation(a1, a2, b1);
            double o2 = Orientation(a1, a2, b2);
            double o3 = Orientation(b1, b2, a1);
            double o4 = Orientation(b1, b2, a2);

            if (o1 * o2 < 0 && o3 * o4 < 0)
                return true;

            if (o1 == 0 && OnSegment(a1, b1, a2)) return true;
            if (o2 == 0 && OnSegment(a1, b2, a2)) return true;
            if (o3 == 0 && OnSegment(b1, a1, b2)) return true;
            if (o4 == 0 && OnSegment(b1, a2, b2)) return true;

            return false;
        }

        private static List<Point> FindLargestConnectedComponent(List<Point> points)
        {
            var visited = new HashSet<Guid>();
            var largestComponent = new List<Point>();

            foreach (var point in points)
            {
                if (!visited.Contains(point.Id))
                {
                    var currentComponent = BFS(point, visited);

                    if (currentComponent.Count > largestComponent.Count)
                    {
                        largestComponent = currentComponent;
                    }
                }
            }

            return largestComponent;
        }

        private static List<Point> BFS(Point startPoint, HashSet<Guid> visited)
        {
            var component = new List<Point>();
            var queue = new Queue<Point>();

            visited.Add(startPoint.Id);
            queue.Enqueue(startPoint);
            component.Add(startPoint);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in current.ConnectedPoints)
                {
                    if (!visited.Contains(neighbor.Id))
                    {
                        visited.Add(neighbor.Id);
                        queue.Enqueue(neighbor);
                        component.Add(neighbor);
                    }
                }
            }

            return component;
        }
    }
}