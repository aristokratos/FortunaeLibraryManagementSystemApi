

using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Service.DTOs;

namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    public interface IBorrowingService
    {
        // Borrowing-related operations
        Task<BorrowingDTO> BorrowBookAsync(Guid userId, Guid bookId);
        Task AddRatingAsync(Guid bookId, Guid userId, int value, string? comment = null);
        Task<List<Borrowing>> GetAllBorrowedBooks();

        // Retrieval operations
        Task<List<BorrowingDTO>> GetMemberBorrowingHistoryAsync(Guid userId);
        Task<List<BorrowingDTO>> GetActiveBorrowingsAsync(Guid userId);
        Task<List<BorrowingDTO>> GetAllBorrowingsAsync();
        Task<List<BorrowingDTO>> GetMemberBorrowedBooksAsync(Guid userId);
        Task<List<BorrowingDTO>> GetOverdueBorrowingsAsync();
        Task<List<BorrowingDTO>> GetAllBorrowedBooksAsync();
        Task ReturnBookAsync(Guid borrowingId, int ratingValue, string? comment = null);

        // Penalty-related operations
        Task PenalizeMemberAsync(Guid borrowingId, decimal penalty);
        Task PenalizeOverdueMembersAsync();

        // Book return and status updates
        Task MarkBookAsReturnedAsync(Guid borrowingId);
        Task<List<BorrowingDTO>> GetBorrowedBooks(Guid userId);
    }

}
