namespace Traffic.Data.Entities
{
    public class TransportEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid MapId { get; set; }

        public Guid PointId { get; set; }
    }
}
