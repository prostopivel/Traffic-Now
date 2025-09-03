namespace Transport.Core.Models
{
    public class Transport
    {
        public Guid Id { get; }

        public Guid MapId { get; }

        public Guid PointId { get; }

        public double X { get; set; }

        public double Y { get; set; }

        public Transport(Guid id, Guid mapId, Guid pointId, double x, double y)
        {
            Id = id;
            MapId = mapId;
            PointId = pointId;
            X = x;
            Y = y;
        }
    }
}
