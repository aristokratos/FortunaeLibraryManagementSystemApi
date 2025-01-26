
namespace FortunaeLibraryManagementSystem.Infrastructure.Repositories
{
    using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using FortunaeLibraryManagementSystem.Infrastructure.Data;
    using Microsoft.EntityFrameworkCore;

    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _dbContext;

        public BookRepository(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Book>> GetBooksAsync(string? filter, string? sortBy, int page, int pageSize)
        {
            IQueryable<Book> query = _dbContext.Books;

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(book =>
                    book.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    book.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    book.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy switch
                {
                    "title" => query.OrderBy(book => book.Title),
                    "author" => query.OrderBy(book => book.Author),
                    "genre" => query.OrderBy(book => book.Genre),
                    _ => query
                };
            }

            // Apply pagination
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(Guid id)
        {
            return await _dbContext.Books.FindAsync(id);
        }
        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            return await _dbContext.Books.Where(book => book.IsAvailable).ToListAsync();
        }

        public async Task AddBookAsync(Book book)
        {
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            _dbContext.Books.Update(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(Book book)
        {
            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();
        }

    }
}
