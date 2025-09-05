using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transport.Core.Abstractions;
using Transport.Core.Models;

namespace Transport.Data.Repositories
{
    public class MapRepository : IMapRepository
    {
        private readonly string _path = string.Empty;

        public MapRepository(IConfiguration configuration)
        {
            _path = string.IsNullOrEmpty(configuration["MapPath"])
                ? Path.GetDirectoryName(Environment.ProcessPath)! + "\\Files"
                : configuration["MapPath"]!;

            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            try
            {
                _path = Directory.GetFiles(_path)
                    .Where(f => f.Split('\\').Last().StartsWith("map_"))
                    .First();
            }
            catch
            {
                throw new FileNotFoundException($"Нужен файл карты в директории '{_path}'");
            }
        }

        public Map GetMap()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };


            var jsonText = File.ReadAllText(_path);
            var map = JsonSerializer.Deserialize<Map>(jsonText, options)
                ?? throw new JsonException($"Can not convert json text to {typeof(Map)}");

            return map;
        }
    }
}
