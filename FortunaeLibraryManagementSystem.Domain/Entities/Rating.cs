
namespace FortunaeLibraryManagementSystem.Domain.Entities
{
    public class Rating
    {
        public Guid Id { get; set; } 
        public Guid BookId { get; set; } 
        public Guid UserId { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Book Book { get; set; }
        public User User { get; set; }
    }

}
