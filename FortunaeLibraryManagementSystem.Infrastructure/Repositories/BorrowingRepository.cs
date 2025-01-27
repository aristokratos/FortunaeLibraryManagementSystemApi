
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
            var borrowings = await _dbContext.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId && b.ReturnedAt == null)
                .ToListAsync();

            if (borrowings == null || borrowings.Count == 0)
            {
                return new List<Borrowing>();
            }

            return borrowings;
        }
        public async Task<List<Borrowing>> GetBorrowedBooks(Guid userId)
        {
            var borrowedBooks = await _dbContext.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == userId && b.ReturnedAt == null) 
                .ToListAsync();

            if (borrowedBooks == null || !borrowedBooks.Any())
            {
                throw new Exception("No borrowed books available."); 
            }


            if (!borrowedBooks.Any())
            {
                throw new Exception("No borrowed books available.");
            }

            return borrowedBooks;
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
            var borrowings = await _dbContext.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowedAt)
                .ToListAsync();

            foreach (var borrowing in borrowings)
            {
                if (borrowing.Book == null)
                {
                    return null;
                }

                if (borrowing.User == null)
                {
                    return null;   
                }
            }

            return borrowings;
        }


        public async Task<List<Borrowing>> GetOverdueBorrowingsAsync()
        {
            return await _dbContext.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.ExpectedReturnDate < DateTime.UtcNow && b.ReturnedAt == null) 
                .ToListAsync();
        }

        public async Task<List<Borrowing>> GetAllActiveBorrowingsAsync()
        {
            return await _dbContext.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.ReturnedAt == null) 
                .ToListAsync();
        }

    }
}
