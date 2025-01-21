
using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Infrastructure.Data;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace FortunaeLibraryManagementSystem.Infrastructure.Repositories
{
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly LibraryDbContext _dbContext;

        public BorrowingRepository(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddBorrowingAsync(Borrowing borrowing)
        {
            await _dbContext.Borrowings.AddAsync(borrowing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateBorrowingAsync(Borrowing borrowing)
        {
            _dbContext.Borrowings.Update(borrowing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Borrowing> GetBorrowingByIdAsync(Guid id)
        {
            return await _dbContext.Borrowings
                .Include(b => b.Book) 
                .Include(b => b.User) 
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Borrowing>> GetActiveBorrowingsByUserAsync(Guid userId)
        {
            return await _dbContext.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId && b.ReturnedAt == null) // Active borrowings
                .ToListAsync();
        }

        public async Task<List<Borrowing>> GetBorrowingHistoryByUserAsync(Guid userId)
        {
            return await _dbContext.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowedAt)
                .ToListAsync();
        }

        public async Task<List<Borrowing>> GetAllBorrowingsAsync()
        {
            return await _dbContext.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowedAt)
                .ToListAsync();
        }
    }
}
