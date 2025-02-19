

using Microsoft.AspNetCore.Http;

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class CreateBookDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
