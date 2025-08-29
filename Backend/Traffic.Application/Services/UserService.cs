using Traffic.Core.Abstractions.Repositories;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetByEmailAsync(string userEmail)
        {
            return await _userRepository.GetByEmailAsync(userEmail);
        }

        public async Task<(Guid?, string Error)> CreateAsync(User user)
        {
            return await _userRepository.CreateAsync(user);
        }
        public async Task<Guid?> UpdateAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<Guid?> DeleteAsync(Guid userId)
        {
            return await _userRepository.DeleteAsync(userId);
        }
    }
}
