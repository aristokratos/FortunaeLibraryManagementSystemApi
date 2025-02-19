

namespace FortunaeLibraryManagementSystem.Infrastructure.Interfaces
{
    using FortunaeLibraryManagementSystem.Domain.Entities;
    public interface IBookRepository
    {
        IQueryable<Book> GetBooksAsync(string? filter, string? sortBy);
        Task<Book> GetBookByIdAsync(Guid id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(Book book);
        Task<List<Book>> GetAvailableBooksAsync();
    }
}
