using Traffic.Core.Entities;

namespace Traffic.Core.Models
{
    public class Map
    {
        public Guid Id { get; }

        public string Name { get; set; } = string.Empty;

        public List<Point> Points { get; } = new List<Point>();

        public Map()
        {
        }

        public Map(MapEntity? mapEntity)
        {
            if (mapEntity == null)
            {
                return;
            }

            Id = mapEntity.MapId;
            Name = mapEntity.MapName;
        }

        private Map(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddPoint(Point point)
        {
            Points.Add(point);
        }

        public static (Map? map, string Error) Create(Guid id, string name)
        {
            var Error = string.Empty;
            var map = new Map(id, name);

            return (map, Error);
        }
    }
}
