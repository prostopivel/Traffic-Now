namespace Traffic.Data.Services
{
    internal interface IDatabaseService
    {
        Task<bool> CheckConnectionAsync();
        Task InitializeDatabaseAsync();
    }
}
