using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using FortunaeLibraryManagementSystem.Service.DTO;

namespace FortunaeLibraryManagementSystem.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public AuthService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterAsync(RegisterDTO registerDto)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(registerDto.Username);
            if (existingUser != null)
                throw new InvalidOperationException(Message.UserNameAlreadyExists);

            var existingEmail = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException(Message.EmailAlreadyExists);

            var passwordHash = HashPassword(registerDto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Role = registerDto.Role,
                Name = registerDto.Name,
                DateOfBirth = registerDto.DateOfBirth,
                ProfileSummary = registerDto.ProfileSummary
            };

            await _userRepository.AddUserAsync(user);
            return true;
        }

        public async Task<string> LoginAsync(string identifier, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(identifier) ??
                       await _userRepository.GetUserByEmailAsync(identifier);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedAccessException(Message.InvalidCredentials);

            return GenerateJwtToken(user.Username, user.Role, user.Id.ToString());
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException(Message.UserNotFound);

            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDTO profileDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(Message.UserNotFound);

            user.Name = profileDto.Name ?? user.Name;
            user.DateOfBirth = profileDto.DateOfBirth ?? user.DateOfBirth;
            user.ProfileSummary = profileDto.ProfileSummary ?? user.ProfileSummary;

            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException(Message.UserNotFound);

            await _userRepository.DeleteUserAsync(user);
            return true;
        }

        private string GenerateJwtToken(string username, string role, string userId)
        {
            var jwtKey  = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
           

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", userId)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
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
