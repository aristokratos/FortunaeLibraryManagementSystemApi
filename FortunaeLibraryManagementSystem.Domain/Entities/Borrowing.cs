
namespace FortunaeLibraryManagementSystem.Domain.Entities
{
    public class Borrowing
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = String.Empty;
        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public bool IsOverdue { get; set; }
        public decimal? Penalty { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Book Book { get; set; }
    }
}
