using Traffic.Core.Models;

namespace Traffic.Core.Abstractions.Repositories
{
    public interface IUserRepository
    {
        Task<(Guid?, string Error)> CreateAsync(User user);
        Task<Guid?> DeleteAsync(Guid userId);
        Task<User?> GetByEmailAsync(string userEmail);
        Task<Guid?> UpdateAsync(User user);
    }
}