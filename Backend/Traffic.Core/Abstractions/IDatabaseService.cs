namespace Traffic.Core.Abstractions
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync();
        Task<bool> CheckConnectionAsync();
        Task ApplyMigrationsAsync();
    }
}
