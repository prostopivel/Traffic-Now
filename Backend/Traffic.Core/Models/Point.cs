using Traffic.Core.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Traffic.Core.Models
{
    public class Point
    {
        public Guid Id { get; }

        [JsonIgnore]
        public Guid MapId { get; }

        [JsonIgnore]
        public Map Map { get; set; }

        public double X { get; }

        public double Y { get; }

        [JsonIgnore]
        public string Name { get; set; } = string.Empty;

        public List<Guid> ConnectedPointsIds { get; private set; } = new List<Guid>();

        [JsonIgnore]
        public List<Point> ConnectedPoints { get; private set; } = new List<Point>();

        public Point()
        {
        }

        public Point(PointEntity? pointEntity)
        {
            if (pointEntity == null)
            {
                return;
            }

            Id = pointEntity.Id;
            MapId = pointEntity.MapId;
            X = pointEntity.X;
            Y = pointEntity.Y;
            Name = pointEntity.Name;
        }

        private Point(Guid id, Guid mapId, double x, double y, string name)
        {
            Id = id;
            MapId = mapId;
            X = x;
            Y = y;
            Name = name;
        }

        public void ConnectPoint(Point point)
        {
            if (ConnectedPoints.Contains(point))
            {
                return;
            }

            ConnectedPoints.Add(point);
            ConnectedPointsIds.Add(point.Id);
            point.ConnectPoint(this);
        }

        public void DisconnectPoint(Guid id)
        {
            if (!ConnectedPointsIds.Contains(id))
            {
                return;
            }

            ConnectedPointsIds.Remove(id);
            ConnectedPoints.Remove(ConnectedPoints.First(p => p.Id == id));
        }

        public void ConnectPoints(IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                ConnectPoint(point);
            }
        }

        public static (Point? point, string Error) Create(Guid id, Guid mapId, double x, double y, string name = "")
        {
            var Error = string.Empty;
            Point? point = null;

            if (x < 0 || y < 0 || x > 100 || y > 100)
            {
                Error = "Координаты должны быть в диапазоне [0, 100]\n";
            }
            else
            {
                point = new Point(id, mapId, x, y, name);
            }

            return (point, Error);
        }
    }
}
