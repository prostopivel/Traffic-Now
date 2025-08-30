using Traffic.Core.Entities;

namespace Traffic.Core.Models
{
    public class Transport
    {
        public Guid Id { get; }

        public Guid UserId { get; }

        public User User { get; private set; }

        public Guid PointId { get; set; }

        public User Point { get; private set; }

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
        }

        private Transport(Guid id, Guid userId, Guid pointId, double startX, double startY)
        {
            Id = id;
            UserId = userId;
            PointId = pointId;
            X = startX;
            Y = startY;
        }

        public static (Transport? transport, string Error) Create(Guid id, Guid userId, Guid pointId, double startX, double startY)
        {
            var Error = string.Empty;
            Transport? transport = null;

            if (startX < 0 || startY < 0 || startX > 100 || startY > 100)
            {
                Error = "Координаты должны быть в диапазоне [0, 100]\n";
            }
            else
            {
                transport = new Transport(id, userId, pointId, startX, startY);
            }

            return (transport, Error);
        }
    }
}
