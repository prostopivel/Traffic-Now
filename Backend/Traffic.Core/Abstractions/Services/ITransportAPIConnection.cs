using Microsoft.AspNetCore.SignalR.Client;

namespace Traffic.Core.Abstractions.Services
{
    public interface ITransportAPIConnection
    {
        string GetConnectionUrl(string hubUrl);
        Task<string?> AddConnectionAsync(string connectionId, string hubUrl);
        IEnumerable<string> GetActiveConnectionIds();
        HubConnection? GetConnection(string connectionId);
        Task<bool> RemoveConnectionAsync(string connectionId);
        Task StopAllConnectionsAsync();
        bool IsTransportActive(string connectionId, int timeoutSeconds = 10)ж
    }
}