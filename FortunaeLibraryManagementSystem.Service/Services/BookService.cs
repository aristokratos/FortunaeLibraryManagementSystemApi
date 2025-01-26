
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

    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly ILogger<BookService> _logger;
        private readonly IImageService _imageService;
        private readonly IMemoryCache _memoryCache;

        public BookService(IBookRepository bookRepository, ILogger<BookService> logger, IImageService imageService, IMemoryCache memoryCache, IRatingRepository ratingRepository)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _imageService = imageService;
            _memoryCache = memoryCache;
            _ratingRepository = ratingRepository;
        }

        public async Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto)
        {
            try
            {
                if (createBookDto.Image == null)
                {
                    throw new ArgumentException("Image cannot be null.", nameof(createBookDto.Image));
                }

                string imageName = await _imageService.UploadImageAsync(createBookDto.Image);

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
                string imageName = await _imageService.UploadImageAsync(updateBookDto.Image);
                book.BookImage = imageName;
            }

            book.Title = updateBookDto.Title ?? book.Title;
            book.Author = updateBookDto.Author ?? book.Author;
            book.Genre = updateBookDto.Genre ?? book.Genre;
            book.Description = updateBookDto.Description ?? book.Description;
            book.ISBN = updateBookDto.ISBN ?? book.ISBN;
            book.IsAvailable = updateBookDto.IsAvailable ?? book.IsAvailable;

            await _bookRepository.UpdateBookAsync(book);

            _memoryCache.Remove("AllBooks");
            _memoryCache.Remove("AvailableBooks");

            return MapToBookDTO(book);
        }

        public async Task DeleteBookAsync(Guid id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                throw new KeyNotFoundException("Book not found");

            await _bookRepository.DeleteBookAsync(book);

            _memoryCache.Remove("AllBooks");
            _memoryCache.Remove("AvailableBooks");
        }

        public async Task<BookDTO> GetBooksByIdAsync(Guid bookId)
        {
            if (!_memoryCache.TryGetValue($"Book_{bookId}", out BookDTO cachedBook))
            {
                var book = await _bookRepository.GetBookByIdAsync(bookId);
                if (book == null)
                {
                    throw new KeyNotFoundException($"Book with ID {bookId} not found.");
                }

                cachedBook = MapToBookDTO(book);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                _memoryCache.Set($"Book_{bookId}", cachedBook, cacheOptions);
            }

            return cachedBook;
        }


        public async Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false)
        {
            if (!_memoryCache.TryGetValue("AllBooks", out List<BookDTO> cachedBooks))
            {
                var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
                cachedBooks = books.Select(MapToBookDTO).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                _memoryCache.Set("AllBooks", cachedBooks, cacheOptions);
            }

            return includeUnavailable
                ? cachedBooks
                : cachedBooks.Where(b => b.IsAvailable).ToList();
        }

        public async Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null)
        {
            if (!_memoryCache.TryGetValue("AvailableBooks", out List<BookDTO> cachedBooks))
            {
                var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
                books = books.Where(b => b.IsAvailable).ToList();

                cachedBooks = books.Select(MapToBookDTO).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                _memoryCache.Set("AvailableBooks", cachedBooks, cacheOptions);
            }

            return !string.IsNullOrWhiteSpace(filter)
                ? cachedBooks.Where(b => b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                         b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                         b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                : cachedBooks;
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
        public async Task<List<BookDTO>> GetCachedTopRatedBooksAsync()
        {
            if (!_memoryCache.TryGetValue("TopRatedBooks", out List<BookDTO> cachedBooks))
            {
                var books = await GetTopRatedBooksAsync();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };
                _memoryCache.Set("TopRatedBooks", books, cacheOptions);
                return books;
            }

            return cachedBooks;
        }
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
                BookImage = book.BookImage
            };
        }
    }

}
