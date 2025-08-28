using Dapper;
using Npgsql;
using System.Data;
using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Entities;
using Traffic.Core.Models;

namespace Traffic.Data.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public UserRepository(IDbConnection connection) : base(connection)
        { }

        public async Task<User?> GetByEmailAsync(string userEmail)
        {
            const string sql = "SELECT * FROM get_user_by_email(@UserEmail)";
            var result = await _connection.QueryAsync<UserEntity>(sql, new { UserEmail = userEmail });
            return new User(result.FirstOrDefault());
        }

        public async Task<(Guid?, string Error)> CreateAsync(User user)
        {
            try
            {
                const string sql = "SELECT create_user(@Id, @Email, @Password, @IsAdmin)";
                var result = await _connection.ExecuteScalarAsync<Guid>(sql, new
                {
                    user.Id,
                    user.Email,
                    user.Password,
                    user.IsAdmin
                });
                return (result, string.Empty);
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001" && ex.MessageText.Contains("already exists"))
            {
                return (null, $"Пользователь с почтой {user.Email} уже существует!");
            }
        }

        public async Task<Guid?> UpdateAsync(User user)
        {
            const string sql = "SELECT update_user(@Id, @Email, @Password)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                user.Id,
                user.Email,
                user.Password
            });
        }

        public async Task<Guid?> DeleteAsync(Guid userId)
        {
            const string sql = "SELECT delete_user(@UserId)";
            return await _connection.ExecuteScalarAsync<Guid>(sql, new { UserId = userId });
        }
    }
}
