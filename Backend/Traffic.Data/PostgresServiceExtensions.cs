using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using Traffic.Data.Options;
using Traffic.Data.Services;

namespace Traffic.Data
{
    public static class PostgresServiceExtensions
    {
        public static IServiceCollection AddPostgresDatabase(
            this IServiceCollection services,
            Action<PostgresDBOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new PostgresDBOptionsBuilder();
            configureOptions(optionsBuilder);
            var options = optionsBuilder.Build();

            services.AddSingleton(options);

            services.AddScoped<IDatabaseService, PostgresDBService>();

            services.AddScoped<IDbConnection>(provider =>
            {
                var options = provider.GetRequiredService<PostgresDBOptions>();
                var connection = new NpgsqlConnection(options.ConnectionString);
                connection.Open();
                return connection;
            });

            ConfigureDapper();

            if (options.InitializeDB)
            {
                services.AddHostedService<DatabaseInitializerHostedService>();
            }

            return services;
        }

        private static void ConfigureDapper()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
        }
    }
}
