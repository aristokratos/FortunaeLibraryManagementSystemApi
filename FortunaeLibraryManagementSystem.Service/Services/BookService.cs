
namespace FortunaeLibraryManagementSystem.Service.Services
{
    using FortunaeLibraryManagementSystem.Service.DTOs;
    using FortunaeLibraryManagementSystem.Service.Interfaces;
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;


    public class BookService : IBookService
    {
    
            private readonly IBookRepository _bookRepository;

            public BookService(IBookRepository bookRepository)
            {
                _bookRepository = bookRepository;
            }

            public async Task<BookDTO> AddBookAsync(CreateBookDTO createBookDto)
            {
                var book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = createBookDto.Title,
                    Author = createBookDto.Author,
                    Genre = createBookDto.Genre,
                    ISBN = createBookDto.ISBN,
                    IsAvailable = true // New books are available by default
                };

                await _bookRepository.AddBookAsync(book);
                return MapToBookDTO(book);
            }

            public async Task<BookDTO> UpdateBookAsync(Guid id, UpdateBookDTO updateBookDto)
            {
                var book = await _bookRepository.GetBookByIdAsync(id);
                if (book == null)
                    throw new KeyNotFoundException("Book not found");

                // Update fields
                book.Title = updateBookDto.Title ?? book.Title;
                book.Author = updateBookDto.Author ?? book.Author;
                book.Genre = updateBookDto.Genre ?? book.Genre;
                book.ISBN = updateBookDto.ISBN ?? book.ISBN;
                book.IsAvailable = updateBookDto.IsAvailable ?? book.IsAvailable;

                await _bookRepository.UpdateBookAsync(book);
                return MapToBookDTO(book);
            }

            public async Task DeleteBookAsync(Guid id)
            {
                var book = await _bookRepository.GetBookByIdAsync(id);
                if (book == null)
                    throw new KeyNotFoundException("Book not found");

                await _bookRepository.DeleteBookAsync(book);
            }

        public async Task<List<BookDTO>> GetAllBooksAsync(bool includeUnavailable = false)
        {
            var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);

            if (!includeUnavailable)
            {
                books = books.Where(b => b.IsAvailable).ToList();
            }

            return books.Select(book => new BookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                ISBN = book.ISBN,
                IsAvailable = book.IsAvailable
            }).ToList();
        }

        public async Task<List<BookDTO>> GetAvailableBooksAsync(string? filter = null)
            {
            var books = await _bookRepository.GetBooksAsync(null, null, 1, 10);

                // Filter by availability
                books = books.Where(b => b.IsAvailable).ToList();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    books = books.Where(b =>
                        b.Title.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        b.Genre.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return books.Select(MapToBookDTO).ToList();
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
                    IsAvailable = book.IsAvailable
                };
            }
        
    }
}
