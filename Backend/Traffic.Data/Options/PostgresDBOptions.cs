namespace Traffic.Data.Options
{
    public class PostgresDBOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool InitializeDB { get; set; } = true;
    }
}
