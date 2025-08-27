namespace Traffic.Data.Options
{
    public class PostgresDBOptionsBuilder
    {
        private readonly PostgresDBOptions _options = new();

        public PostgresDBOptionsBuilder WithConnectionString(string connectionString)
        {
            _options.ConnectionString = connectionString;
            return this;
        }

        public PostgresDBOptionsBuilder WithInitializeDB(bool isInitializeDB)
        {
            _options.InitializeDB = isInitializeDB;
            return this;
        }

        internal PostgresDBOptions Build()
        {
            if (string.IsNullOrEmpty(_options.ConnectionString))
                throw new InvalidOperationException("Требуется строка подключения");

            return _options;
        }
    }
}
