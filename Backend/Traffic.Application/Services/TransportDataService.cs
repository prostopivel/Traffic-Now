using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.DataStructures;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class TransportDataService : ITransportDataService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Guid, TransportData> _transportData = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentIndexedSet<Guid>> _userTransport = new();

        public TransportDataService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ConcurrentDictionary<Guid, ConcurrentIndexedSet<Guid>> UserTransport => _userTransport;

        public Dictionary<Guid, TransportData> this[Guid userId]
        {
            get
            {
                if (_userTransport.TryGetValue(userId, out var transportList))
                {
                    return transportList.GetAll()
                        .Select(transportId => KeyValuePair.Create(transportId, _transportData.TryGetValue(transportId, out var data) ? data : null))
                        .Where(data => data.Value != null)
                        .ToDictionary()!;
                }
                return [];
            }
        }

        public async Task AddTransport(Guid transportId, TransportData data)
        {
            bool wasAdded = _transportData.TryAdd(transportId, data);

            if (wasAdded)
            {
                using var scope = _serviceProvider.CreateScope();
                var transportService = scope.ServiceProvider.GetRequiredService<ITransportService>();
                var usersId = await transportService.GetTransportUsersAsync(transportId);

                if (usersId == null) return;

                foreach (var userId in usersId)
                {
                    var transportList = _userTransport.GetOrAdd(userId, _ => []);
                    transportList.Add(transportId);
                }
            }
            else
            {
                _transportData[transportId] = data;
            }
        }

        public Task AddUser(Guid userId, Guid transportId)
        {
            var transportList = _userTransport.GetOrAdd(userId, _ => []);
            transportList.Add(transportId);

            return Task.CompletedTask;
        }

        public async Task RemoveTransport(Guid transportId)
        {
            if (!_transportData.TryRemove(transportId, out _))
                return;

            using var scope = _serviceProvider.CreateScope();
            var transportService = scope.ServiceProvider.GetRequiredService<ITransportService>();
            var usersId = await transportService.GetTransportUsersAsync(transportId);

            if (usersId == null) return;

            foreach (var userId in usersId)
            {
                if (_userTransport.TryGetValue(userId, out var transportList))
                {
                    transportList.Remove(transportId);

                    if (transportList.Count == 0)
                    {
                        _userTransport.TryRemove(userId, out _);
                    }
                }
            }
        }

        public async Task RemoveInactiveTransport()
        {
            foreach (var item in _transportData)
            {
                if (item.Value.LastUpdate < DateTime.Now.AddSeconds(-10))
                {
                    await RemoveTransport(item.Key);
                }
            }
        }

        public Task RemoveTransportUser(Guid userId, Guid transportId)
        {
            if (_userTransport.TryGetValue(userId, out var transportList))
            {
                transportList.Remove(transportId);

                if (transportList.Count == 0)
                {
                    _userTransport.TryRemove(userId, out _);
                }
            }

            return Task.CompletedTask;
        }
    }
}