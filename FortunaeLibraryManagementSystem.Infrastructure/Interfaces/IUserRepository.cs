

using FortunaeLibraryManagementSystem.Domain.Entities;

namespace FortunaeLibraryManagementSystem.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task AddUserAsync(User user);
        Task<User> GetUserByIdAsync(Guid id);
        Task DeleteUserAsync(User user);
    }
}
