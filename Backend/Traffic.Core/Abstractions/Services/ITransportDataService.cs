using System.Collections.Concurrent;
using Traffic.Core.DataStructures;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public interface ITransportDataService
    {
        ConcurrentDictionary<Guid, ConcurrentIndexedSet<Guid>> UserTransport { get; }
        Dictionary<Guid, TransportData> this[Guid userId] { get; }
        Task AddTransport(Guid transportId, TransportData data);
        Task AddUser(Guid userId, Guid transportId);
        Task RemoveInactiveTransport();
        Task RemoveTransport(Guid transportId);
        Task RemoveTransportUser(Guid userId, Guid transportId);
    }
}