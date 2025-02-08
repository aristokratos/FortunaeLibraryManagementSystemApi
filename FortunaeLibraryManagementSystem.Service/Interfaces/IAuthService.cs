

using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Service.DTO;
using FortunaeLibraryManagementSystem.Service.DTOs;

namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(RegisterDTO registerDto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<User> GetUserByIdAsync(Guid id);
        Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDTO profileDto);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
}
