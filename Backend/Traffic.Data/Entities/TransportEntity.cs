namespace Traffic.Data.Entities
{
    public class TransportEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
