

using FortunaeLibraryManagementSystem.Domain.Enums;

namespace FortunaeLibraryManagementSystem.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public UserRoleEnum Role { get; set; }
    }
}
