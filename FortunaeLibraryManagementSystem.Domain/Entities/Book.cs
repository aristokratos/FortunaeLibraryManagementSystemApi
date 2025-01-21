

namespace FortunaeLibraryManagementSystem.Domain.Entities
{
    public class Book
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public string? ISBN { get; set; }
        public bool IsAvailable { get; set; }
    }
}
