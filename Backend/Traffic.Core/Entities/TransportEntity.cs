namespace Traffic.Core.Entities
{
    public class TransportEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid PointId { get; set; }

        public string Url { get; set; }
    }
}
