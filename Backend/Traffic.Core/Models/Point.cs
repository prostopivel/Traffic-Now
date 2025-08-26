namespace Traffic.Core.Models
{
    internal class Point
    {
        public Guid Id { get; }

        public Guid MapId { get; }

        public double X { get; }

        public double Y { get; }

        public string Name { get; set; } = string.Empty;

        public List<Point> ConnectedPoints { get; private set; } = new List<Point>();

        public Point()
        {
        }

        private Point(Guid id, Guid mapId, double x, double y)
        {
            Id = id;
            MapId = mapId;
            X = x;
            Y = y;
        }

        public void ConnectPoint(Point point)
        {
            if (ConnectedPoints.Contains(point))
            {
                return;
            }

            ConnectedPoints.Add(point);
            point.ConnectPoint(this);
        }

        public static (Point? point, string Error) Create(Guid id, Guid mapId, double x, double y)
        {
            var Error = string.Empty;
            Point? point = null;

            if (x < 0 || y < 0 || x > 100 || y > 100)
            {
                Error = "Координаты должны быть в диапазоне [0, 100]\n";
            }
            else
            {
                point = new Point(id, mapId, x, y);
            }

            return (point, Error);
        }
    }
}
