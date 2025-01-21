

namespace FortunaeLibraryManagementSystem.Infrastructure.Interfaces
{
    using FortunaeLibraryManagementSystem.Domain.Entities;
    public interface IBookRepository
    {
        Task<List<Book>> GetBooksAsync(string? filter, string? sortBy, int page, int pageSize);
        Task<Book> GetBookByIdAsync(Guid id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(Book book);

    }
}
