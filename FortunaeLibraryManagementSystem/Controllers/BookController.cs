

namespace FortunaeLibraryManagementSystem.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using FortunaeLibraryManagementSystem.Service.Interfaces;
    using FortunaeLibraryManagementSystem.Service.DTOs;
    using FortunaeLibraryManagementSystem.Domain.Constants;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Adds a new book.
        /// </summary>
        /// <param name="createBookDto">The book details.</param>
        /// <returns>The created book with its ID.</returns>
        [HttpPost("AddBook")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromForm] CreateBookDTO createBookDto)
        {
            var book = await _bookService.AddBookAsync(createBookDto);
            return CreatedAtAction(nameof(GetAllBooksForAdmin), new { id = book.Id }, book);
        }

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="id">The ID of the book to update.</param>
        /// <param name="updateBookDto">Updated book details.</param>
        /// <returns>The updated book.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(Guid id, [FromForm] UpdateBookDTO updateBookDto)
        {
            var book = await _bookService.UpdateBookAsync(id, updateBookDto);
            return Ok(book);
        }



        /// <summary>
        /// Gets all books (including unavailable ones) for admin users.
        /// </summary>
        /// <returns>A list of all books.</returns>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBooksForAdmin()
        {
            var books = await _bookService.GetAllBooksAsync(includeUnavailable: true);
            return Ok(books);
        }

        /// <summary>
        /// Deletes a book by ID.
        /// </summary>
        /// <param name="bookId">The ID of the book to delete.</param>
        /// <returns>Success or failure message.</returns>
        [HttpDelete("book/{bookId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBookById(Guid bookId)
        {
            var result = await _bookService.DeleteBookAsync(bookId);
            if (result)
            {
                return Ok(Message.BookDeleted);
            }
            return NotFound(Message.BookNotFound);
        }

        /// <summary>
        /// Gets all available books, optionally filtered by title, author, or genre.
        /// </summary>
        /// <param name="filter">Optional search filter.</param>
        /// <returns>A list of available books.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableBooks([FromQuery] string? filter)
        {
            var books = await _bookService.GetAvailableBooksAsync(filter);
            return Ok(books);
        }

        /// <summary>
        /// Searches books based on title, author, genre, and availability.
        /// </summary>
        /// <param name="title">Book title.</param>
        /// <param name="author">Book author.</param>
        /// <param name="genre">Book genre.</param>
        /// <param name="isAvailable">Availability status.</param>
        /// <returns>Matching books.</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchBooks(
           [FromQuery] string? title = null,
           [FromQuery] string? author = null,
           [FromQuery] string? genre = null,
           [FromQuery] bool? isAvailable = null)
        {
            var books = await _bookService.SearchBooksAsync(title, author, genre, isAvailable);
            return Ok(books);
        }
        
        /// <summary>
        /// Get a book by its ID.
        /// </summary>
        /// <param name="bookId">The ID of the book to retrieve.</param>
        /// <returns>A `BookDTO` representing the book details.</returns>
        /// 
        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetBookById(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                return BadRequest(Message.InvalidBookId);
            }

            try
            {
                var book = await _bookService.GetBooksByIdAsync(bookId);
                return Ok(book);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the top-rated books.
        /// </summary>
        /// <param name="top">Number of books to return (default is 10).</param>
        /// <returns>A list of top-rated books.</returns>
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedBooks([FromQuery] int top = 10)
        {
            var books = await _bookService.GetTopRatedBooksAsync(top);
            return Ok(books);
        }
        /// <summary>
        /// Retrieves cached top-rated books.
        /// </summary>
        /// <returns>A list of cached top-rated books.</returns>
        [HttpGet("top-rated/cached")]
        public async Task<IActionResult> GetCachedTopRatedBooks()
        {
            var books = await _bookService.GetCachedTopRatedBooksAsync();
            return Ok(books);
        }



        /// <summary>
        /// Retrieves books related to a specific book.
        /// </summary>
        /// <param name="bookId">The ID of the book to find related books for.</param>
        /// <returns>A list of related books.</returns>
        [HttpGet("{bookId}/related")]
        public async Task<IActionResult> GetRelatedBooks(Guid bookId)
        {
            var relatedBooks = await _bookService.GetRelatedBooksAsync(bookId);
            return Ok(relatedBooks);
        }

        /// <summary>
        /// Gets ratings given by a specific user.
        /// </summary>
        /// <param name="bookId">The ID of the user.</param>
        /// <returns>A list of ratings given by the user.</returns>
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetRatingsByBookId(Guid bookId)
        {
            var ratings = await _bookService.GetRatingsByBookIdAsync(bookId);
            return Ok(ratings);
        }

        /// <summary>
        /// Gets ratings given by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of ratings given by the user.</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRatingsByUserId(Guid userId)
        {
            var ratings = await _bookService.GetRatingsByUserIdAsync(userId);
            return Ok(ratings);
        }

    }
}
