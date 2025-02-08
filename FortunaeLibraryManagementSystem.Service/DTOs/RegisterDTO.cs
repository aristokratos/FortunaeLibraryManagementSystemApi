
namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Role { get; set; } // "Admin" or "Member"
        public string? Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileSummary { get; set; }
    }
}
