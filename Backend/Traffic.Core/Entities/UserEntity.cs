using Traffic.Core.Models;

namespace Traffic.Core.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }
    }
}
