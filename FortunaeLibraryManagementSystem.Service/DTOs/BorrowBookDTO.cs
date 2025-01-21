
namespace FortunaeLibraryManagementSystem.Service.DTOs
{
    public class BorrowBookDTO
    {
        public Guid BookId { get; set; }
    }

    public class ReturnBookDTO
    {
        public Guid BorrowingId { get; set; }
    }

    public class BorrowingDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public string? BookTitle { get; set; }
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public bool IsOverdue { get; set; }
        public decimal? Penalty { get; set; }
    }
}
