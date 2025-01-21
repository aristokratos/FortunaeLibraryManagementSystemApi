
namespace FortunaeLibraryManagementSystem.Service.Services
{
    using FortunaeLibraryManagementSystem.Service.DTOs;
    using FortunaeLibraryManagementSystem.Service.Interfaces;
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
    using Microsoft.Extensions.Caching.Distributed;
    using System.Text.Json;




    public class BookService : IBookService
    {
    
        private readonly IBookRepository _bookRepository;
        private readonly IDistributedCache _cache;
        public BookService(IBookRepository bookRepository, IDistributedCache cache)
        {
            _bookRepository = bookRepository;
            _cache = cache;
        }


        public async Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto)
        {
            string? imageBase64 = null;

            // Handle image upload
            if (createBookDto.Image != null)
            {
                // Save the image to wwwroot/images
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", createBookDto.Image.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await createBookDto.Image.CopyToAsync(stream);
                }

                // Convert image to Base64
                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                imageBase64 = Convert.ToBase64String(imageBytes);
            }

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = createBookDto.Title,
                Author = createBookDto.Author,
                Genre = createBookDto.Genre,
                ISBN = createBookDto.ISBN,
                IsAvailable = true,
                BookImage = imageBase64 
            };

            await _bookRepository.AddBookAsync(book);
            await _cache.RemoveAsync("AllBooks");
            await _cache.RemoveAsync("AvailableBooks");
            return MapToBookDTO(book);
        }


        public async Task<BookDTO> UpdateBookAsync(Guid id, UpdateBookDTO updateBookDto)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                throw new KeyNotFoundException("Book not found");

            if (updateBookDto.Image != null)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", updateBookDto.Image.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await updateBookDto.Image.CopyToAsync(stream);
                }

                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                book.BookImage = Convert.ToBase64String(imageBytes);
            }

            book.Title = updateBookDto.Title ?? book.Title;
            book.Author = updateBookDto.Author ?? book.Author;
            book.Genre = updateBookDto.Genre ?? book.Genre;
            book.ISBN = updateBookDto.ISBN ?? book.ISBN;
            book.IsAvailable = updateBookDto.IsAvailable ?? book.IsAvailable;

            await _bookRepository.UpdateBookAsync(book);
            await _cache.RemoveAsync("AllBooks");
            await _cache.RemoveAsync("AvailableBooks");
            return MapToBookDTO(book);
        }


            public async Task DeleteBookAsync(Guid id)
                {
                    var book = await _bookRepository.GetBookByIdAsync(id);
                    if (book == null)
                        throw new KeyNotFoundException("Book not found");

                    await _bookRepository.DeleteBookAsync(book);
                    await _cache.RemoveAsync("AllBooks");
                    await _cache.RemoveAsync("AvailableBooks");
        }

        public async Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false)
        {
            // Check if the data is in Redis cache
            var cacheKey = "AllBooks";
            var cachedBooks = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedBooks))
            {
                // Return cached data if available
                var cachedBookList = JsonSerializer.Deserialize<List<BookDTO>>(cachedBooks);
                return includeUnavailable
                    ? cachedBookList
                    : cachedBookList.Where(b => b.IsAvailable).ToList();
            }

            // Fetch from the database if not cached
            var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
            var bookDTOs = books.Select(MapToBookDTO).ToList();

            // Cache the data in Redis
            var serializedData = JsonSerializer.Serialize(bookDTOs);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expires in 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            return includeUnavailable
                ? bookDTOs
                : bookDTOs.Where(b => b.IsAvailable).ToList();
        }

        public async Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null)
        {
            // Check if the data is in Redis cache
            var cacheKey = "AvailableBooks";
            var cachedBooks = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedBooks))
            {
                // Return cached data if available
                var cachedBookList = JsonSerializer.Deserialize<List<BookDTO>>(cachedBooks);
                return !string.IsNullOrWhiteSpace(filter)
                    ? cachedBookList.Where(b => b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                                b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                                b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                    : cachedBookList;
            }

            // Fetch from the database if not cached
            var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);
            books = books.Where(b => b.IsAvailable).ToList();

            var bookDTOs = books.Select(MapToBookDTO).ToList();

            // Cache the data in Redis
            var serializedData = JsonSerializer.Serialize(bookDTOs);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expires in 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            // Apply filtering
            return !string.IsNullOrWhiteSpace(filter)
                ? bookDTOs.Where(b => b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                      b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                      b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList()
                : bookDTOs;
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
