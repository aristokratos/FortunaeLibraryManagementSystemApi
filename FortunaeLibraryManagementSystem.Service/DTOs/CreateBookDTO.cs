

using Microsoft.AspNetCore.Http;

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class CreateBookDTO
    {
        public string Title { get; set; } 
        public string Author { get; set; }
        public string Genre { get; set; }
        public string ISBN { get; set; }
        public IFormFile? Image { get; set; }
    }
}
