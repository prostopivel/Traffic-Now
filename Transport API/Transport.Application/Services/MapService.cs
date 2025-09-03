using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Transport.Core.Abstractions;

namespace Transport.Application.Services
{
    public class MapService : IHostedService
    {
        private readonly IDataService _dataService;
        private readonly IServiceProvider _serviceProvider;

        public MapService(IDataService dataService, IServiceProvider serviceProvider)
        {
            _dataService = dataService;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var mapRepository = scope.ServiceProvider.GetRequiredService<IMapRepository>();
                var map = mapRepository.GetMap();
                _dataService.Map = map;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
