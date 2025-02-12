
namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    using FortunaeLibraryManagementSystem.Service.DTOs;


    public interface IBookService
    {
        Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto);
        Task<BookDTO> UpdateBookAsync(Guid id, UpdateBookDTO updateBookDto);
        Task<bool> DeleteBookAsync(Guid id);
        Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false); 
        Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null);
        Task<BookDTO> GetBooksByIdAsync(Guid bookId);
        
        //Task AddBookRatingAsync(Guid bookId, int rating);
        Task AddRatingAsync(Guid bookId, Guid userId, int value, string? comment = null);
        Task<List<BookDTO>> GetTopRatedBooksAsync(int top = 10);
       // Task<List<BookDTO>> GetCachedTopRatedBooksAsync();
        Task<List<BookDTO>> SearchBooksAsync(string? title = null, string? author = null, string? genre = null, bool? isAvailable = null);
        Task<List<BookDTO>> GetRelatedBooksAsync(Guid bookId);
        Task<List<RatingDTO>> GetRatingsByUserIdAsync(Guid userId);
        Task<List<RatingDTO>> GetRatingsByBookIdAsync(Guid bookId);
    }
}
