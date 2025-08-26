using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace Traffic.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IDbConnection>(provider =>
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                return connection;
            });

            ConfigureDapper();

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
