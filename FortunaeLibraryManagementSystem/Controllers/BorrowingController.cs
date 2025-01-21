using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookDTO borrowBookDto)
        {
            var userId = Guid.Parse(User.Identity.Name); // Extract user ID from JWT claims
            var borrowing = await _borrowingService.BorrowBookAsync(userId, borrowBookDto);
            return Ok(borrowing);
        }

        [HttpPut("{id}/return")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> ReturnBook(Guid id)
        {
            await _borrowingService.ReturnBookAsync(id);
            return NoContent();
        }

        [HttpGet("history")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetBorrowingHistory()
        {
            var userId = Guid.Parse(User.Identity.Name);
            var history = await _borrowingService.GetMemberBorrowingHistoryAsync(userId);
            return Ok(history);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetActiveBorrowings()
        {
            var userId = Guid.Parse(User.Identity.Name);
            var activeBorrowings = await _borrowingService.GetActiveBorrowingsAsync(userId);
            return Ok(activeBorrowings);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBorrowings()
        {
            var borrowings = await _borrowingService.GetAllBorrowingsAsync();
            return Ok(borrowings);
        }

        [HttpPut("{id}/penalize")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PenalizeMember(Guid id, [FromQuery] decimal penalty)
        {
            await _borrowingService.PenalizeMemberAsync(id, penalty);
            return NoContent();
        }

        [HttpPut("{id}/mark-returned")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkBookAsReturned(Guid id)
        {
            await _borrowingService.MarkBookAsReturnedAsync(id);
            return NoContent();
        }
    }
}
