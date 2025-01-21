

using FortunaeLibraryManagementSystem.Service.DTOs;

namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> AuthenticateAsync(string username, string password);
    }
}
