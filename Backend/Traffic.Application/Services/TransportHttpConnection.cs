using System.Text.Json;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.API.Services
{
    public class TransportHttpConnection : ITransportHttpConnection
    {
        public async Task<TransportJsonResponse?> GetTransport(string url)
        {
            url += "/api/transport";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var content = await response.Content.ReadAsStringAsync();
            var transportResponse = JsonSerializer.Deserialize<TransportJsonResponse>(content, options)!;

            return transportResponse;
        }
    }
}
