namespace Traffic.Data.Entities
{
    public class PointEntity
    {
        public Guid Id { get; set; }
        public Guid MapId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
