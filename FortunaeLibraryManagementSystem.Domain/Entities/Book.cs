

namespace FortunaeLibraryManagementSystem.Domain.Entities
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        public bool IsAvailable { get; set; }
        public string? BookImage { get; set; }
        public decimal AverageRating { get; set; } 

        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
