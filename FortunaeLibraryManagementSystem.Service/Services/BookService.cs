
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
        private readonly ILogger<BookService> _logger;
        private readonly IImageService _imageService;
        private readonly IMemoryCache _memoryCache;

        public BookService(IBookRepository bookRepository, ILogger<BookService> logger, IImageService imageService, IMemoryCache memoryCache)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _imageService = imageService;
            _memoryCache = memoryCache;
        }


        public async Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto)
        {
            try
            {
                if (createBookDto.Image == null)
                {
                    throw new ArgumentException("Image cannot be null.", nameof(createBookDto.Image));
                }

                string imageUrl = await _imageService.UploadImageAsync(createBookDto.Image);

                var book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = createBookDto.Title,
                    Author = createBookDto.Author,
                    Genre = createBookDto.Genre,
                    ISBN = createBookDto.ISBN,
                    IsAvailable = true,
                    BookImage = imageUrl // Save the Cloudinary URL in the database
                };

                // Save the book details to the repository
                await _bookRepository.AddBookAsync(book);

                // Optionally clear cached data (if caching is used)
                //await _cache.RemoveAsync("AllBooks");
                //await _cache.RemoveAsync("AvailableBooks");

                // Map and return the book details
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
                // Upload the image to Cloudinary using the image service
                string imageUrl = await _imageService.UploadImageAsync(updateBookDto.Image);
                book.BookImage = imageUrl;
            }

            book.Title = updateBookDto.Title ?? book.Title;
            book.Author = updateBookDto.Author ?? book.Author;
            book.Genre = updateBookDto.Genre ?? book.Genre;
            book.ISBN = updateBookDto.ISBN ?? book.ISBN;
            book.IsAvailable = updateBookDto.IsAvailable ?? book.IsAvailable;

            await _bookRepository.UpdateBookAsync(book);

            // Remove cache for updated data
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

            // Remove cache for updated data
            _memoryCache.Remove("AllBooks");
            _memoryCache.Remove("AvailableBooks");
        }

        public async Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false)
        {
            // Check if the data is in-memory cache
            if (!_memoryCache.TryGetValue("AllBooks", out List<BookDTO> cachedBooks))
            {
                // Fetch from the database if not cached
                var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
                cachedBooks = books.Select(MapToBookDTO).ToList();

                // Cache the data in-memory
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expires in 10 minutes
                };
                _memoryCache.Set("AllBooks", cachedBooks, cacheOptions);
            }

            return includeUnavailable
                ? cachedBooks
                : cachedBooks.Where(b => b.IsAvailable).ToList();
        }

        public async Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null)
        {
            // Check if the data is in-memory cache
            if (!_memoryCache.TryGetValue("AvailableBooks", out List<BookDTO> cachedBooks))
            {
                // Fetch from the database if not cached
                var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
                books = books.Where(b => b.IsAvailable).ToList();

                cachedBooks = books.Select(MapToBookDTO).ToList();

                // Cache the data in-memory
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expires in 10 minutes
                };
                _memoryCache.Set("AvailableBooks", cachedBooks, cacheOptions);
            }

            // Apply filtering
            return !string.IsNullOrWhiteSpace(filter)
                ? cachedBooks.Where(b => b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                         b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                         b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                : cachedBooks;
        }



        private BookDTO MapToBookDTO(Book book)
            {
                return new BookDTO
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Genre = book.Genre,
                    ISBN = book.ISBN,
                    IsAvailable = book.IsAvailable,
                    BookImage = book.BookImage
                };
            }

    }
}
