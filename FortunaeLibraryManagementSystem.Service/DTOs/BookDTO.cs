

namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class BookDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string BookImage { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
    }
}
