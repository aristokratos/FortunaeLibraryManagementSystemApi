using Azure.Core;
using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FortunaeLibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;

        public BorrowingController(IBorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> BorrowBook(Guid userId, Guid bookId)
        {
            try
            {
                var borrowingDto = await _borrowingService.BorrowBookAsync(userId, bookId);
                return Ok(borrowingDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Specific error when book or borrowing record is not found
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Generic error handler for unexpected issues
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("{id}/return")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> ReturnBook(Guid borrowingId, [FromBody] ReturnBookRequest request)
        {
            try
            {
                await _borrowingService.ReturnBookAsync(borrowingId, request.RatingValue, request.Comment);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                // Book or borrowing record not found
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetBorrowingHistory()
        {
            try
            {
                var userId = Guid.Parse(User.Identity.Name);
                var history = await _borrowingService.GetMemberBorrowingHistoryAsync(userId);
                return Ok(history);
            }
            catch (FormatException ex)
            {
                // Handle invalid GUID format if needed
                return BadRequest(new { message = "Invalid user identifier.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetActiveBorrowings()
        {
            try
            {
               
                var activeBorrowings = await _borrowingService.GetAllBorrowingsAsync();
                return Ok(activeBorrowings);
            }
            catch (FormatException ex)
            {
                return BadRequest(new { message = "Invalid user identifier.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBorrowings()
        {
            try
            {
                var borrowings = await _borrowingService.GetAllBorrowingsAsync();
                return Ok(borrowings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("{id}/penalize")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PenalizeMember(Guid id, [FromQuery] decimal penalty)
        {
            try
            {
                await _borrowingService.PenalizeMemberAsync(id, penalty);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                // Borrowing record not found
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("{id}/mark-returned")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkBookAsReturned(Guid id)
        {
            try
            {
                await _borrowingService.MarkBookAsReturnedAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                // Borrowing record not found
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

    }
}
