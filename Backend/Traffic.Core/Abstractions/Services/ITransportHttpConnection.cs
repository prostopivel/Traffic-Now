using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Services
{
    public interface ITransportHttpConnection
    {
        Task<TransportJsonResponse?> GetTransport(string url);
    }
}