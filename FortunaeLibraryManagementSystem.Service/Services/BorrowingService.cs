

using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Domain.Entities;

namespace FortunaeLibraryManagementSystem.Service.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookRepository _bookRepository;

        public BorrowingService(IBorrowingRepository borrowingRepository, IBookRepository bookRepository)
        {
            _borrowingRepository = borrowingRepository;
            _bookRepository = bookRepository;
        }

        public async Task<BorrowingDTO> BorrowBookAsync(Guid userId, BorrowBookDTO borrowBookDto)
        {
            var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsByUserAsync(userId);
            if (activeBorrowings.Count >= 3)
                throw new InvalidOperationException("Members can only borrow up to 3 books at a time.");

            var book = await _bookRepository.GetBookByIdAsync(borrowBookDto.BookId);
            if (book == null || !book.IsAvailable)
                throw new InvalidOperationException("The requested book is not available for borrowing.");

            // Create a new borrowing record
            var borrowing = new Borrowing
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BookId = borrowBookDto.BookId,
                BorrowedAt = DateTime.UtcNow,
                IsOverdue = false
            };

            // Mark the book as unavailable
            book.IsAvailable = false;

            await _borrowingRepository.AddBorrowingAsync(borrowing);
            await _bookRepository.UpdateBookAsync(book);

            return MapToBorrowingDTO(borrowing, book.Title);
        }

        public async Task ReturnBookAsync(Guid borrowingId)
        {
            var borrowing = await _borrowingRepository.GetBorrowingByIdAsync(borrowingId);
            if (borrowing == null)
                throw new KeyNotFoundException("Borrowing record not found.");

            borrowing.ReturnedAt = DateTime.UtcNow;

            var book = await _bookRepository.GetBookByIdAsync(borrowing.BookId);
            if (book != null)
                book.IsAvailable = true;

            await _borrowingRepository.UpdateBorrowingAsync(borrowing);
            await _bookRepository.UpdateBookAsync(book);
        }

        public async Task<List<BorrowingDTO>> GetMemberBorrowingHistoryAsync(Guid userId)
        {
            var borrowings = await _borrowingRepository.GetBorrowingHistoryByUserAsync(userId);
            return borrowings.Select(b => MapToBorrowingDTO(b)).ToList();
        }

        public async Task<List<BorrowingDTO>> GetActiveBorrowingsAsync(Guid userId)
        {
            var borrowings = await _borrowingRepository.GetActiveBorrowingsByUserAsync(userId);
            return borrowings.Select(b => MapToBorrowingDTO(b)).ToList();
        }

        public async Task<List<BorrowingDTO>> GetAllBorrowingsAsync()
        {
            var borrowings = await _borrowingRepository.GetAllBorrowingsAsync();
            return borrowings.Select(b => MapToBorrowingDTO(b)).ToList();
        }

        public async Task PenalizeMemberAsync(Guid borrowingId, decimal penalty)
        {
            var borrowing = await _borrowingRepository.GetBorrowingByIdAsync(borrowingId);
            if (borrowing == null)
                throw new KeyNotFoundException("Borrowing record not found.");

            borrowing.Penalty = penalty;

            await _borrowingRepository.UpdateBorrowingAsync(borrowing);
        }

        public async Task MarkBookAsReturnedAsync(Guid borrowingId)
        {
            var borrowing = await _borrowingRepository.GetBorrowingByIdAsync(borrowingId);
            if (borrowing == null)
                throw new KeyNotFoundException("Borrowing record not found.");

            borrowing.ReturnedAt = DateTime.UtcNow;
            borrowing.IsOverdue = false;

            var book = await _bookRepository.GetBookByIdAsync(borrowing.BookId);
            if (book != null)
                book.IsAvailable = true;

            await _borrowingRepository.UpdateBorrowingAsync(borrowing);
            await _bookRepository.UpdateBookAsync(book);
        }

        private BorrowingDTO MapToBorrowingDTO(Borrowing borrowing, string bookTitle = null)
        {
            return new BorrowingDTO
            {
                Id = borrowing.Id,
                UserId = borrowing.UserId,
                BookId = borrowing.BookId,
                BookTitle = bookTitle,
                BorrowedAt = borrowing.BorrowedAt,
                ReturnedAt = borrowing.ReturnedAt,
                IsOverdue = borrowing.IsOverdue,
                Penalty = borrowing.Penalty
            };
        }
    }
}
