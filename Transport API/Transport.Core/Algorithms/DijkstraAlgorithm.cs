using Transport.Core.Models;

namespace Transport.Core.Algorithms
{
    public class DijkstraAlgorithm
    {
        public static List<Point>? FindShortestPath(Map map, Point start, Point target)
        {
            var distances = new Dictionary<Guid, double>();
            var previous = new Dictionary<Guid, Point?>();
            var unvisited = new HashSet<Point>();

            foreach (var point in map.Points)
            {
                distances[point.Id] = point.Id == start.Id ? 0 : double.MaxValue;
                previous[point.Id] = null;
                unvisited.Add(point);
            }

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(p => distances[p.Id]).FirstOrDefault();
                if (current == null || distances[current.Id] == double.MaxValue)
                    break;

                if (current.Id == target.Id)
                {
                    return ReconstructPath(previous, target);
                }

                unvisited.Remove(current);

                foreach (var neighbor in GetConnectedPoints(map, current))
                {
                    if (!unvisited.Contains(neighbor))
                        continue;

                    double distanceToNeighbor = CalculateDistance(current, neighbor);
                    double alt = distances[current.Id] + distanceToNeighbor;

                    if (alt < distances[neighbor.Id])
                    {
                        distances[neighbor.Id] = alt;
                        previous[neighbor.Id] = current;
                    }
                }
            }

            return null;
        }

        private static List<Point> ReconstructPath(Dictionary<Guid, Point?> previous, Point target)
        {
            var path = new List<Point>();
            Point? current = target;

            while (current != null)
            {
                path.Add(current);
                current = previous.TryGetValue(current.Id, out Point? value)
                    ? value
                    : null;
            }

            path.Reverse();
            return path;
        }

        private static double CalculateDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
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
