namespace Traffic.Core.Abstractions.Services
{
    public interface IConnectionTransportService
    {
        Task<bool> ConnectUserTransport(Guid userId);
        Task<bool> DisconnectUserTransport(Guid userId);
    }
}