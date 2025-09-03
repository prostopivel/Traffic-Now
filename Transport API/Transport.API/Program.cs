using Transport.API.Hubs;
using Transport.API.Services;
using Transport.Application.Services;
using Transport.Core.Abstractions;
using Transport.Data.Repositories;

namespace Transport.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            ConfigureRepositories(builder);
            ConfigureRepositoryServices(builder);

            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            builder.Services.AddHostedService<MapService>();
            builder.Services.AddHostedService<TransportService>();
            builder.Services.AddHostedService<SendDataBackgroungService>();
            builder.Services.AddHostedService<TransportHostService>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.MapControllers();
            app.MapHub<TransportHub>("/transportHub");

            app.Run();
        }

        private static void ConfigureRepositories(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IDataRepository, DataRepository>();
            builder.Services.AddSingleton<IRouteRepository, RouteRepository>();
            builder.Services.AddScoped<IMapRepository, MapRepository>();
            builder.Services.AddScoped<ITransportRepository, TransportRepository>();
        }

        private static void ConfigureRepositoryServices(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IDataService, DataService>();
            builder.Services.AddSingleton<IRouteService, RouteService>();
        }
    }
}
