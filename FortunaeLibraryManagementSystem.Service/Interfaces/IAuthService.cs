

using FortunaeLibraryManagementSystem.Service.DTOs;

namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(RegisterDTO registerDto);
        Task<bool> DeleteUserAsync(Guid id);
    }
}
