

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Use a registered email address or username")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "The Password field is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "The password must be at least 8 characters long and include at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
