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

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public MapRepository(IConfiguration configuration)
        {
            var appBasePath = AppContext.BaseDirectory;

            _path = string.IsNullOrEmpty(configuration["MapPath"])
                ? Path.Combine(appBasePath, "Files")
                : configuration["MapPath"]!;

            if (!Directory.Exists(_path))
            {
                if (string.IsNullOrEmpty(configuration["MapPath"]))
                {
                    throw new DirectoryNotFoundException($"Директория с файлами карт не найдена: '{_path}'. Убедитесь, что файлы скопировались при публикации.");
                }
                Directory.CreateDirectory(_path);
            }

            try
            {
                var files = Directory.GetFiles(_path)
                    .Where(f => Path.GetFileName(f).StartsWith("map_"))
                    .ToList();

                if (files.Count == 0)
                {
                    throw new FileNotFoundException($"В директории '{_path}' не найдено файлов карт (начинающихся с 'map_')");
                }

                _path = files.First();
            }
            catch (Exception ex) when (ex is not FileNotFoundException)
            {
                throw new FileNotFoundException($"Ошибка при поиске файлов карт в директории '{_path}'", ex);
            }
        }

        public Map GetMap()
        {
            var jsonText = File.ReadAllText(_path);
            var map = JsonSerializer.Deserialize<Map>(jsonText, _jsonSerializerOptions)
                ?? throw new JsonException($"Can not convert json text to {typeof(Map)}");

            return map;
        }
    }
}
