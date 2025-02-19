
using System.ComponentModel.DataAnnotations;

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "username cannot be empty")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "The Password field is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "The password must be at least 8 characters long and include at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Invalid user role, user can either be Admin or Member")]
        public string Role { get; set; } = string.Empty;
        [Required(ErrorMessage ="Please input your full name")]
        public string Name { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
        public string? ProfileSummary { get; set; }
    }
}
