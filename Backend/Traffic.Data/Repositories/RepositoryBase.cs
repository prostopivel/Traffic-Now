using System.Data;

namespace Traffic.Data.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly IDbConnection _connection;

        protected RepositoryBase(IDbConnection connection)
        {
            _connection = connection;
        }
    }
}
