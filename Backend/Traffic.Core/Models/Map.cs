namespace Traffic.Core.Models
{
    internal class Map
    {
        public Guid Id { get; }

        public string Name { get; set; } = string.Empty;

        public List<Point> Points { get; } = new List<Point>();

        public Map()
        {
        }

        private Map(Guid id, string name, List<Point> points)
        {
            Id = id;
            Name = name;
            Points = points;
        }

        public static (Map? map, string Error) Create(Guid id, string name, List<Point> points)
        {
            var Error = string.Empty;
            var map = new Map(id, name, points);

            return (map, Error);
        }
    }
}
