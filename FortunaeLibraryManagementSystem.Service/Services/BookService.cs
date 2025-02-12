
namespace FortunaeLibraryManagementSystem.Service.Services
{
    using FortunaeLibraryManagementSystem.Service.DTOs;
    using FortunaeLibraryManagementSystem.Service.Interfaces;
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
    using Microsoft.Extensions.Caching.Distributed;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Caching.Memory;
    using FortunaeLibraryManagementSystem.Service.Services.CacheService;

    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly ILogger<BookService> _logger;
        private readonly IImageService _imageService;
        //private readonly IMemoryCache _memoryCache;
        private readonly IRedisService _cache;
        private const int CACHE_DURATION_MINUTES = 10;

        public BookService(IBookRepository bookRepository, ILogger<BookService> logger, IImageService imageService, IRedisService cache, IRatingRepository ratingRepository)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _imageService = imageService;
            //_memoryCache = memoryCache;
            _ratingRepository = ratingRepository;
            _cache = cache;
        }

        public async Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto)
        {
            try
            {
                if (createBookDto.Image == null)
                {
                    throw new ArgumentException("Image cannot be null.", nameof(createBookDto.Image));
                }

                // Use ImageUrlResponseDto instead of string
                ImageUrlResponseDto imageResponse = await _imageService.UploadImageAsync(createBookDto.Image);
                string imageName = imageResponse.PresignedUrl;

                var book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = createBookDto.Title,
                    Author = createBookDto.Author,
                    Genre = createBookDto.Genre,
                    ISBN = createBookDto.ISBN,
                    Description = createBookDto.Description,
                    IsAvailable = true,
                    BookImage = imageName
                };

                await _bookRepository.AddBookAsync(book);

                return MapToBookDTO(book);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while adding the book: {Exception}", ex);
                throw new Exception("An unexpected error occurred while adding the book.", ex);
            }
        }

        public async Task<BookDTO> UpdateBookAsync(Guid id, UpdateBookDTO updateBookDto)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                throw new KeyNotFoundException("Book not found");

            if (updateBookDto.Image != null)
            {
                // Use ImageUrlResponseDto instead of string
                ImageUrlResponseDto imageResponse = await _imageService.UploadImageAsync(updateBookDto.Image);
                book.BookImage = imageResponse.PresignedUrl;
            }

            book.Title = updateBookDto.Title ?? book.Title;
            book.Author = updateBookDto.Author ?? book.Author;
            book.Genre = updateBookDto.Genre ?? book.Genre;
            book.Description = updateBookDto.Description ?? book.Description;
            book.ISBN = updateBookDto.ISBN ?? book.ISBN;
            book.IsAvailable = updateBookDto.IsAvailable ?? book.IsAvailable;

            await _bookRepository.UpdateBookAsync(book);

            return MapToBookDTO(book);
        }


        public async Task<BookDTO> GetBooksByIdAsync(Guid bookId)
        {
            string cacheKey = $"Book_{bookId}";

            var cachedBook = await _cache.GetAsync<BookDTO>(cacheKey);
            if (cachedBook != null)
                return cachedBook;

            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null)
                throw new KeyNotFoundException($"Book with ID {bookId} not found.");

            var bookDto = MapToBookDTO(book);
            await _cache.SetAsync(cacheKey, bookDto, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

            return bookDto;
        }



        public async Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false)
        {
            string cacheKey = "AllBooks";

            var cachedBooks = await _cache.GetAsync<List<BookDTO>>(cacheKey);
            if (cachedBooks != null)
                return includeUnavailable ? cachedBooks : cachedBooks.Where(b => b.IsAvailable).ToList();

            var booksList = await _bookRepository.GetBooksAsync(null, null, 1, 10);
            var bookDtos = booksList.Select(MapToBookDTO).ToList();

            await _cache.SetAsync(cacheKey, bookDtos, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

            return includeUnavailable ? bookDtos : bookDtos.Where(b => b.IsAvailable).ToList();
        }

        public async Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null)
        {
            string cacheKey = "AvailableBooks";

            var cachedBooks = await _cache.GetAsync<List<BookDTO>>(cacheKey);
            if (cachedBooks != null)
            {
                return !string.IsNullOrWhiteSpace(filter)
                    ? cachedBooks.Where(b => b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                           b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                           b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                    : cachedBooks;
            }

            var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
            var bookDtos = books.Where(b => b.IsAvailable).Select(MapToBookDTO).ToList();

            await _cache.SetAsync(cacheKey, bookDtos, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            return !string.IsNullOrWhiteSpace(filter)
                ? bookDtos.Where(b => b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                    b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                    b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                : bookDtos;
        }
        public async Task AddRatingAsync(Guid bookId, Guid userId, int value, string? comment = null)
        {
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                UserId = userId,
                Value = value,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            await _ratingRepository.AddRatingAsync(rating);

            var ratings = await _ratingRepository.GetRatingsByBookIdAsync(bookId);
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            Console.WriteLine($"Ratings: {ratings.Count()}"); 
            if (ratings.Any())
            {
                book.AverageRating = (decimal)ratings.Average(r => r.Value);
            }
            else
            {
                book.AverageRating = 0;  
            }

            
            book.AverageRating = (decimal)ratings.Average(r => r.Value);

            await _bookRepository.UpdateBookAsync(book);
        }



        public async Task<List<BookDTO>> GetTopRatedBooksAsync(int top = 10)
        {
            var books = await _bookRepository.GetBooksAsync(null, null, 1, 100);
            return books
                .OrderByDescending(b => b.AverageRating)
                .Take(top)
                .Select(MapToBookDTO)
                .ToList();
        }
        //public async Task<List<BookDTO>> GetCachedTopRatedBooksAsync()
        //{
        //    string cacheKey = "TopRatedBooks";

        //    var cachedBooks = await _cache.GetAsync<List<BookDTO>>(cacheKey);
        //    if (cachedBooks != null)
        //        return cachedBooks;

        //    var books = await GetTopRatedBooksAsync();
        //    await _cache.SetAsync(cacheKey, books, TimeSpan.FromMinutes(15));

        //    return books;
        //}

        public async Task<List<BookDTO>> SearchBooksAsync(string? title = null, string? author = null, string? genre = null, bool? isAvailable = null)
        {
            var books = await _bookRepository.GetBooksAsync(null, null, 1, 100);

            if (!string.IsNullOrWhiteSpace(title))
                books = books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(author))
                books = books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(genre))
                books = books.Where(b => b.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase)).ToList();

            if (isAvailable.HasValue)
                books = books.Where(b => b.IsAvailable == isAvailable.Value).ToList();

            return books.Select(MapToBookDTO).ToList();
        }

        public async Task<List<BookDTO>> GetRelatedBooksAsync(Guid bookId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null)
                throw new KeyNotFoundException("Book not found.");

            var relatedBooks = await _bookRepository.GetBooksAsync(book.Genre, book.Author, 1, 10);
            return relatedBooks.Where(b => b.Id != bookId).Select(MapToBookDTO).ToList();
        }
        public async Task<List<RatingDTO>> GetRatingsByBookIdAsync(Guid bookId)
        {
            var ratings = await _ratingRepository.GetRatingsByBookIdAsync(bookId);
            return ratings.Select(r => new RatingDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                Value = r.Value,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
        public async Task<List<RatingDTO>> GetRatingsByUserIdAsync(Guid userId)
        {
            var ratings = await _ratingRepository.GetRatingsByUserIdAsync(userId);
            return ratings.Select(r => new RatingDTO
            {
                Id = r.Id,
                BookId = r.BookId,
                Value = r.Value,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<bool> DeleteBookAsync(Guid bookId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID {bookId} not found.");
                return false;
            }

            await _bookRepository.DeleteBookAsync(book);
            await InvalidateBookCaches(bookId);

            _logger.LogInformation($"Book with ID {bookId} deleted and cache invalidated.");
            return true;
        }



        private async Task InvalidateBookCaches(Guid bookId)
        {
            var cacheKeys = new[]
            {
                "AllBooks",
                "AvailableBooks",
                "TopRatedBooks",
                $"Book_{bookId}"
            };

            foreach (var key in cacheKeys)
            {
                await _cache.RemoveAsync(key);
            }
        }
        private BookDTO MapToBookDTO(Book book)
        {
            return new BookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                Description = book.Description,
                ISBN = book.ISBN,
                IsAvailable = book.IsAvailable,
                BookImage = book.BookImage,
                AverageRating = book.AverageRating
            };
        }
    }

}
