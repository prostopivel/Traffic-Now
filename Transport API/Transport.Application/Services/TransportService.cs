using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Transport.Core.Abstractions;

namespace Transport.Application.Services
{
    public class TransportService : IHostedService
    {
        private readonly IDataService _dataService;
        private readonly IServiceProvider _serviceProvider;

        public TransportService(IDataService dataService, IServiceProvider serviceProvider)
        {
            _dataService = dataService;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var transportRepository = scope.ServiceProvider.GetRequiredService<ITransportRepository>();
                transportRepository.CreateTransport(_dataService);
                var transport = transportRepository.GetTransport();

                _dataService.Transport = transport;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
