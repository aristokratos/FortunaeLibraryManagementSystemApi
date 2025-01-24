
using FortunaeLibraryManagementSystem.Domain.Enums;

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRoleEnum Role { get; set; }
    }
}
