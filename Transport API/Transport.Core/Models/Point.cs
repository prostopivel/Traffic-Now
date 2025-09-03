namespace Transport.Core.Models
{
    [Serializable]
    public class Point
    {
        public Guid Id { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public List<Guid> ConnectedPointsIds { get; set; } = new List<Guid>();
    }
}
