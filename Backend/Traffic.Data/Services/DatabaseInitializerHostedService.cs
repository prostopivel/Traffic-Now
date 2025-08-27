using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Traffic.Data.Services
{
    internal class DatabaseInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializerHostedService> _logger;

        public DatabaseInitializerHostedService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseInitializerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

            _logger.LogInformation("Starting database initialization...");

            try
            {
                await databaseService.InitializeDatabaseAsync();
                _logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}