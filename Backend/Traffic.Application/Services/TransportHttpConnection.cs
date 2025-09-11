using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.API.Services
{
    public class TransportHttpConnection : ITransportHttpConnection
    {
        private readonly ILogger<TransportHttpConnection> _logger;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public TransportHttpConnection(ILogger<TransportHttpConnection> logger)
        {
            _logger = logger;
        }

        public async Task<TransportJsonResponse?> GetTransport(string url)
        {
            url += "/api/transport";

            using var httpClient = new HttpClient();
            HttpResponseMessage? response = null;
            try
            {
                response = await httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Can not connect to {url}! Error message: {message}", url, ex.Message);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var transportResponse = JsonSerializer.Deserialize<TransportJsonResponse>(content, _jsonSerializerOptions)!;

            return transportResponse;
        }

        public async Task<int> GetTransportSpeed(string url)
        {
            url += "/api/transport/getSpeed";

            using var httpClient = new HttpClient();
            HttpResponseMessage? response = null;
            try
            {
                response = await httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Can not connect to {url}! Error message: {message}", url, ex.Message);
                return 0;
            }

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (!int.TryParse(content, out var speed))
            {
                return 0;
            }

            return speed;
        }
    }
}
