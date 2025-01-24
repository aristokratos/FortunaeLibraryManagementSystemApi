using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using FortunaeLibraryManagementSystem.Domain.Enums;

namespace FortunaeLibraryManagementSystem.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        // Hardcoded users (replace this with a database or repository)
        private readonly Dictionary<string, (string Password, string Role)> _users = new()
        {
            { "admin", ("password", "Admin") },
            { "member", ("password", "Member") }
        };

        public AuthService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterAsync(RegisterDTO registerDto)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(registerDto.Username);
            if (existingUser != null)
                throw new InvalidOperationException("Username already exists");

            var passwordHash = HashPassword(registerDto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                Role = registerDto.Role
            };

            await _userRepository.AddUserAsync(user);

            return true;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            return GenerateJwtToken(user.Username, user.Role);
        }


        private string GenerateJwtToken(string username, UserRoleEnum role)
        {
            var jwtKey = _configuration["JwtSettings:SecretKey"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role.ToString()),

            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }




        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hashedPassword;
        }

    }
}
