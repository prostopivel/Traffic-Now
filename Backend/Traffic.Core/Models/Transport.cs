using Traffic.Core.Entities;

namespace Traffic.Core.Models
{
    public class Transport
    {
        public Guid Id { get; }

        public Guid UserId { get; }

        public Guid PointId { get; set; }

        public Point Point { get; set; }

        public bool IsActive { get; set; } = false;

        public string Url { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public Transport()
        {
        }

        public Transport(TransportEntity? transportEntity)
        {
            if (transportEntity == null)
            {
                return;
            }

            Id = transportEntity.Id;
            UserId = transportEntity.UserId;
            PointId = transportEntity.PointId;
            Url = transportEntity.Url;
        }

        private Transport(Guid id, Guid userId, Guid pointId, string url, double startX, double startY)
        {
            Id = id;
            UserId = userId;
            PointId = pointId;
            Url = url;
            X = startX;
            Y = startY;
        }

        public static (Transport? transport, string Error) Create(Guid id, Guid userId, Guid pointId, string url, double startX, double startY)
        {
            var Error = string.Empty;
            Transport? transport = null;

            if (startX < 0 || startY < 0 || startX > 100 || startY > 100)
            {
                Error = "Координаты должны быть в диапазоне [0, 100]\n";
            }
            else
            {
                transport = new Transport(id, userId, pointId, url, startX, startY);
            }

            return (transport, Error);
        }
    }
}
