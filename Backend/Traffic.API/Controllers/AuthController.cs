using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Traffic.API.Contracts;
using Traffic.Core.Abstractions.Services;

namespace Traffic.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.GetByEmailAsync(request.Username);
            if (user == null || user.Id == Guid.Empty)
                return Unauthorized(new { message = "Пользователь не найден!" });

            var serverHash = HashPassword(request.Password, _configuration["Salt"]!);

            if (serverHash != user.Password)
                return Unauthorized(new { message = "Неверный пароль!" });

            var tokenString = GenerateJwtToken(user);

            return Ok(new { Token = tokenString });
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize([FromBody] LoginRequest request)
        {
            var serverHash = HashPassword(request.Password, _configuration["Salt"]!);

            (Core.Models.User? user, string error) = Core.Models.User.Create(
                Guid.NewGuid(),
                request.Username,
                serverHash,
                false);

            if (!string.IsNullOrEmpty(error) || user == null)
                return Unauthorized(error);

            var Error = (await _userService.CreateAsync(user)).Error;
            if (!string.IsNullOrEmpty(Error))
            {
                return Unauthorized(new { message = Error });
            }

            var tokenString = GenerateJwtToken(user);

            return Ok(new { Token = tokenString });
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult Validate()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { userId, message = "Token is valid", ok = true });
        }

        private string GenerateJwtToken(Core.Models.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                ]),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private static string HashPassword(string password, string salt)
        {
            var saltedPassword = salt + password;
            var bytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
