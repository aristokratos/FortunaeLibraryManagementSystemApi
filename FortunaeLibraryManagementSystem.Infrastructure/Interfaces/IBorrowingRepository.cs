

using FortunaeLibraryManagementSystem.Domain.Entities;

namespace FortunaeLibraryManagementSystem.Infrastructure.Interfaces
{
    public interface IBorrowingRepository
    {
        Task AddBorrowingAsync(Borrowing borrowing);
        Task UpdateBorrowingAsync(Borrowing borrowing);
        Task<Borrowing> GetBorrowingByIdAsync(Guid id);
        Task<List<Borrowing>> GetActiveBorrowingsByUserAsync(Guid userId);
        Task<List<Borrowing>> GetBorrowingHistoryByUserAsync(Guid userId);
        Task<List<Borrowing>> GetAllBorrowingsAsync();
    }
}
