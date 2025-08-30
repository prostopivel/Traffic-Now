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
            if (user == null)
                return Unauthorized(new { message = "Неверный пароль!" });

            var serverHash = HashPassword(request.Password, _configuration["Salt"]!);

            if (serverHash != user.Password)
                return Unauthorized();

            var tokenString = GenerateJwtToken(request);

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

            if (!string.IsNullOrEmpty(error))
                return Unauthorized(error);

            await _userService.CreateAsync(user!);

            var tokenString = GenerateJwtToken(request);

            return Ok(new { Token = tokenString });
        }

        private string GenerateJwtToken(LoginRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, request.Username),
                    new Claim(ClaimTypes.Role, "User")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private string HashPassword(string password, string salt)
        {
            var saltedPassword = salt + password;
            var bytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
