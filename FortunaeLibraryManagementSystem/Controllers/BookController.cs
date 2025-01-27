

namespace FortunaeLibraryManagementSystem.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using FortunaeLibraryManagementSystem.Service.Interfaces;
    using FortunaeLibraryManagementSystem.Service.DTOs;

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromForm] CreateBookDTO createBookDto)
        {
            var book = await _bookService.AddBookAsync(createBookDto);
            return CreatedAtAction(nameof(GetAllBooksForAdmin), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(Guid id, [FromForm] UpdateBookDTO updateBookDto)
        {
            var book = await _bookService.UpdateBookAsync(id, updateBookDto);
            return Ok(book);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBooksForAdmin()
        {
            var books = await _bookService.GetAllBooksAsync(includeUnavailable: true);
            return Ok(books);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableBooks([FromQuery] string? filter)
        {
            var books = await _bookService.GetAvailableBooksAsync(filter);
            return Ok(books);
        }
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
        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetBookById(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                return BadRequest("Invalid book ID.");
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
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedBooks([FromQuery] int top = 10)
        {
            var books = await _bookService.GetTopRatedBooksAsync(top);
            return Ok(books);
        }

        [HttpGet("top-rated/cached")]
        public async Task<IActionResult> GetCachedTopRatedBooks()
        {
            var books = await _bookService.GetCachedTopRatedBooksAsync();
            return Ok(books);
        }

       

        [HttpGet("{bookId}/related")]
        public async Task<IActionResult> GetRelatedBooks(Guid bookId)
        {
            var relatedBooks = await _bookService.GetRelatedBooksAsync(bookId);
            return Ok(relatedBooks);
        }
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetRatingsByBookId(Guid bookId)
        {
            var ratings = await _bookService.GetRatingsByBookIdAsync(bookId);
            return Ok(ratings);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRatingsByUserId(Guid userId)
        {
            var ratings = await _bookService.GetRatingsByUserIdAsync(userId);
            return Ok(ratings);
        }
    }
}
