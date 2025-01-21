

namespace FortunaeLibraryManagementSystem.Domain.Entities
{
    public class BorrowingActivity
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Guid UserId { get; set; }
        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedAt { get; set; } = null;
    }
}
