using System.Text.RegularExpressions;
using Traffic.Core.Entities;

namespace Traffic.Core.Models
{
    public class User
    {
        private static readonly Regex _emailRegex = new Regex(
            @"^(?!\.)(?!.*\.\.)([a-zA-Z0-9._%+-]+)(?<!\.)@(?!\.)([a-zA-Z0-9.-]+)(?<!\.)\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public Guid Id { get; }

        public string Email { get; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; }

        public User()
        {
        }

        public User(UserEntity? userEntity)
        {
            if (userEntity == null)
            {
                return;
            }

            Id = userEntity.Id;
            Email = userEntity.Email;
            Password = userEntity.Password;
            IsAdmin = userEntity.IsAdmin;
        }

        private User(Guid id, string email, string password, bool isAdmin)
        {
            Id = id;
            Email = email;
            Password = password;
            IsAdmin = isAdmin;
        }

        public static (User? User, string Error) Create(Guid id, string email, string password, bool isAdmin = false)
        {
            var Error = string.Empty;
            User? User = null;

            if (string.IsNullOrEmpty(email) || !_emailRegex.IsMatch(email))
            {
                Error += "Неверный адрес электронной почты!\n";
            }

            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                Error += "Пароль должен содержать хотя бы 8 символов!\n";
            }

            if (Error == string.Empty)
            {
                User = new User(id, email, password, isAdmin);
            }

            return (User, Error);
        }
    }
}
