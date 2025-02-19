
namespace FortunaeLibraryManagementSystem.Service.Interfaces
{
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using FortunaeLibraryManagementSystem.Service.DTOs;


    public interface IBookService
    {
        Task<ResponseMessages.ApiSuccessResponse<BookDTO>> AddBookAsync(CreateBookDTO createBookDto);
        Task<ResponseMessages.ApiSuccessResponse<BookDTO>> UpdateBookAsync(Guid id, UpdateBookDTO updateBookDto);
        Task<bool> DeleteBookAsync(Guid id);
        Task<ResponseMessages.ApiSuccessResponse<PaginatedList<BookDTO>>> GetAllBooksAsync(bool includeUnavailable = false, int pageNumber = 1, int pageSize = 10);
        Task<PaginatedList<BookDTO>> GetAvailableBooksAsync(string? filter = null, int pageNumber = 1, int pageSize = 10);
        Task<ResponseMessages.ApiSuccessResponse<BookDTO>> GetBooksByIdAsync(Guid bookId);
        
        //Task AddBookRatingAsync(Guid bookId, int rating);
        Task AddRatingAsync(Guid bookId, Guid userId, int value, string? comment = null);
        Task<List<BookDTO>> GetTopRatedBooksAsync(int top = 10);
        Task<List<BookDTO>> GetCachedTopRatedBooksAsync();
        Task<PaginatedList<BookDTO>> SearchBooksAsync(string? title = null, string? author = null, string? genre = null, bool? isAvailable = null, int pageNumber = 1, int pageSize = 10);
        Task<List<BookDTO>> GetRelatedBooksAsync(Guid bookId);
        Task<List<RatingDTO>> GetRatingsByUserIdAsync(Guid userId);
        Task<List<RatingDTO>> GetRatingsByBookIdAsync(Guid bookId);
    }
}
