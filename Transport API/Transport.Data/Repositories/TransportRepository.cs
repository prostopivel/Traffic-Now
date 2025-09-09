using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transport.Core.Abstractions;

namespace Transport.Data.Repositories
{
    public class TransportRepository : ITransportRepository
    {
        private readonly string _path = string.Empty;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public TransportRepository(IConfiguration configuration)
        {
            _path = string.IsNullOrEmpty(configuration["MapPath"])
                ? Path.GetDirectoryName(Environment.ProcessPath)! + "\\Files"
                : configuration["MapPath"]!;

            _path += "\\transport.json";
        }

        public void CreateTransport(IDataService _dataService)
        {
            if (!File.Exists(_path))
            {
                var random = new Random();

                var point = _dataService.Map.Points[random.Next(0, _dataService.Map.Points.Count)];

                var transport = new Core.Models.Transport(
                    Guid.NewGuid(),
                    point.Id,
                    point.X,
                    point.Y);

                var json = JsonSerializer.Serialize(transport, _jsonSerializerOptions);
                File.WriteAllText(_path, json);
            }
        }

        public Core.Models.Transport GetTransport()
        {
            if (File.Exists(_path))
            {
                var jsonText = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<Core.Models.Transport>(jsonText, _jsonSerializerOptions)
                    ?? throw new JsonException($"Can not convert json text to {typeof(Core.Models.Transport)}");
            }
            else
            {
                throw new FileNotFoundException($"File with path '{_path}' not found!");
            }
        }
    }
}
