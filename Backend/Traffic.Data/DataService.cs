using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Reflection;
using System.Text;
using Traffic.Core.Abstractions;

namespace Traffic.Data
{
    public class DataService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DataService> _logger;
        private const string SQL_SCRIPTS_PATH = "SQL";

        public DataService(string connectionString, ILogger<DataService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed");
                return false;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
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

        private async Task ExecuteSqlScriptsAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.Contains(SQL_SCRIPTS_PATH) && name.EndsWith(".sql"))
                .OrderBy(name => name)
                .ToList();

            if (!resourceNames.Any())
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

            if (cleanedScript.Contains("CREATE OR REPLACE FUNCTION") ||
                cleanedScript.Contains("create or replace function"))
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

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        public Task ApplyMigrationsAsync()
        {
            throw new NotImplementedException("Migrations are not available!");
        }

        private IDbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}