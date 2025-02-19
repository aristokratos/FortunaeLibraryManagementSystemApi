
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

        public  IQueryable<Book> GetBooksAsync(string? filter, string? sortBy)
        {
            IQueryable<Book> query = _dbContext.Books;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query =  query.Where(book =>
                    book.Title.Contains(filter) ||
                    book.Author.Contains(filter) ||
                    book.Genre.Contains(filter));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "title" => query.OrderBy(book => book.Title),
                    "author" => query.OrderBy(book => book.Author),
                    "genre" => query.OrderBy(book => book.Genre),
                    "rating" => query.OrderByDescending(book => book.AverageRating),
                    _ => query.OrderBy(book => book.Title)
                };
            }
            else
            {
                query = query.OrderBy(book => book.Title);
            }

            return  query;
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
