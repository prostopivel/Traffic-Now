namespace Traffic.Core.Models
{
    internal class Transport
    {
        public Guid Id { get; }

        public Guid UserId { get; }

        public double X { get; set; }

        public double Y { get; set; }

        public Transport()
        {
        }

        private Transport(Guid id, Guid userId, double startX, double startY)
        {
            Id = id;
            UserId = userId;
            X = startX;
            Y = startY;
        }

        public static (Transport? transport, string Error) Create(Guid id, Guid userId, double startX, double startY)
        {
            var Error = string.Empty;
            Transport? transport = null;

            if (startX < 0 || startY < 0 || startX > 100 || startY > 100)
            {
                Error = "Координаты должны быть в диапазоне [0, 100]\n";
            }
            else
            {
                transport = new Transport(id, userId, startX, startY);
            }

            return (transport, Error);
        }
    }
}
