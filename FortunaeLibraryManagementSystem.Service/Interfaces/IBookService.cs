
namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    using FortunaeLibraryManagementSystem.Service.DTOs;


    public interface IBookService
    {
        Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto);
        Task<BookDTO> UpdateBookAsync(Guid id, UpdateBookDTO updateBookDto);
        Task DeleteBookAsync(Guid id);
        Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false); 
        Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null);
    }
}
