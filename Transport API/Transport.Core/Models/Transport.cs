namespace Transport.Core.Models
{
    public class Transport
    {
        public Guid Id { get; }

        public Guid PointId { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public Transport(Guid id, Guid pointId, double x, double y)
        {
            Id = id;
            PointId = pointId;
            X = x;
            Y = y;
        }
    }
}
