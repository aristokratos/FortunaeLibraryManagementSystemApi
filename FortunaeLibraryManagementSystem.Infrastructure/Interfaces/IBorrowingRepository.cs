

using FortunaeLibraryManagementSystem.Domain.Entities;
using System.Threading.Tasks;

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
        Task<List<Borrowing>> GetBorrowedBooks(Guid userId);
        Task<List<Borrowing>> GetAllBorrowedBooks();
    }
}
