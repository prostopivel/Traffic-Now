namespace Traffic.Core.Models
{
    public class TransportData
    {
        public DateTime LastUpdate { get; private set; }

        public double X { get; set; }

        public double Y { get; set; }

        private TransportData()
        {
        }

        private TransportData(double x, double y)
        {
            X = x;
            Y = y;
            LastUpdate = DateTime.Now;
        }

        public static (TransportData? transportData, string Error) Create(double x, double y)
        {
            var Error = string.Empty;
            TransportData? transportData = new TransportData(x, y);

            return (transportData, Error);
        }
    }
}
