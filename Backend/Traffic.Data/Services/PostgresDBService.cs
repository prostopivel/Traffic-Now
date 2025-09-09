using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Reflection;
using System.Text;
using Traffic.Data.Options;

namespace Traffic.Data.Services
{
    internal class PostgresDBService : IDatabaseService
    {
        private readonly PostgresDBOptions _options;
        private readonly ILogger<PostgresDBService> _logger;
        private const string SQL_MANIFEST_DEFAULT_RESOURCE_NAME = @"SQL\*.sql";

        public PostgresDBService(PostgresDBOptions options, ILogger<PostgresDBService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_options.ConnectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database connection failed");
                return false;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            if (!_options.InitializeDB)
            {
                _logger.LogInformation("The database will not be initialized.");
                return;
            }

            _logger.LogInformation("Initializing database...");

            if (!await CheckConnectionAsync())
            {
                throw new Exception("Database connection failed");
            }

            try
            {
                await ExecuteSqlScriptsAsync();

                _logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }

        private IDbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(_options.ConnectionString);
            connection.Open();
            return connection;
        }

        private async Task ExecuteSqlScriptsAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames().ToList();
            resourceNames =
                [.. (resourceNames.Any(name => name.EndsWith(".sql"))
                    ? resourceNames.Where(name => name.EndsWith(".sql"))
                    : [SQL_MANIFEST_DEFAULT_RESOURCE_NAME])
                .OrderBy(name => name)];

            if (resourceNames.Count == 0)
            {
                throw new KeyNotFoundException("Манифест ресурса SQL не был найден!");
            }

            _logger.LogInformation("Found {Count} SQL scripts in embedded resources", resourceNames.Count);

            using var connection = CreateConnection();

            foreach (var resourceName in resourceNames)
            {
                try
                {
                    _logger.LogInformation("Executing script: {ScriptName}", resourceName);

                    var scriptContent = await ReadEmbeddedResourceAsync(resourceName);
                    await ExecuteScriptAsync(connection, scriptContent, resourceName);

                    _logger.LogInformation("Script executed successfully: {ScriptName}", resourceName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute script: {ScriptName}", resourceName);
                    throw;
                }
            }
        }

        private async Task ExecuteScriptAsync(IDbConnection connection, string scriptContent, string scriptName)
        {
            var lines = scriptContent.Split('\n')
                .Where(line => !line.TrimStart().StartsWith("--"))
                .ToArray();

            var cleanedScript = string.Join("\n", lines);

            if (cleanedScript.Contains("CREATE OR REPLACE FUNCTION", StringComparison.CurrentCultureIgnoreCase)
                || cleanedScript.Contains("CREATE FUNCTION", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    _logger.LogDebug("Executing function script as single command");
                    await connection.ExecuteAsync(cleanedScript);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute function script {ScriptName}", scriptName);
                    throw;
                }
            }

            var commands = cleanedScript.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(cmd => cmd.Trim())
                .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
                .ToList();

            foreach (var command in commands)
            {
                if (string.IsNullOrWhiteSpace(command) || command.StartsWith("--"))
                    continue;

                try
                {
                    _logger.LogDebug("Executing command: {Command}", command);
                    await connection.ExecuteAsync(command);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute command in script {ScriptName}: {Command}",
                        scriptName, command);
                    throw;
                }
            }
        }

        private async Task<string> ReadEmbeddedResourceAsync(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Встроенный ресурс не найден: {resourceName}");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}