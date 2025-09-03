namespace Transport.Core.Models
{
    [Serializable]
    public class Map
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Point> Points { get; set; } = new List<Point>();
    }
}
