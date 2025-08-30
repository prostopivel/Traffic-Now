using Traffic.Core.Entities;

namespace Traffic.Core.Models
{
    public class Route
    {
        public Guid Id { get; }

        public Guid TransportId { get; }

        public Transport Transport { get; set; }

        public DateTime RouteTime { get; set; }

        public Queue<Point> Points { get; private set; } = new Queue<Point>();

        public Route()
        {
        }

        public Route(RouteEntity? routeEntity)
        {
            if (routeEntity == null)
            {
                return;
            }

            Id = routeEntity.Id;
            TransportId = routeEntity.TransportId;
            RouteTime = routeEntity.RouteTime;
        }

        private Route(Guid id, Guid transportId, DateTime routeTime)
        {
            Id = id;
            TransportId = transportId;
            RouteTime = routeTime;
        }

        public void EnqueuePoints(IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                Points.Enqueue(point);
            }
        }

        public static (Route? route, string Error) Create(Guid id, Guid transportId, DateTime routeTime)
        {
            var Error = string.Empty;
            Route? route = new Route(id, transportId, routeTime);

            return (route, Error);
        }
    }
}
