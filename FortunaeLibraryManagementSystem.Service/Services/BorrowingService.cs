

using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Domain.Entities;
using System;
using FortunaeLibraryManagementSystem.Infrastructure.Repositories;

namespace FortunaeLibraryManagementSystem.Service.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IRatingRepository _ratingRepository;

        public BorrowingService(IBorrowingRepository borrowingRepository, IBookRepository bookRepository, IRatingRepository ratingRepository)
        {
            _borrowingRepository = borrowingRepository;
            _bookRepository = bookRepository;
            _ratingRepository = ratingRepository;
        }

        public async Task<BorrowingDTO> BorrowBookAsync(Guid userId, Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bookId), "The borrow book data cannot be null.");
            }

            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                throw new InvalidOperationException("The requested book does not exist.");
            }

            if (!book.IsAvailable)
            {
                throw new InvalidOperationException("The requested book is not available for borrowing.");
            }

            var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsByUserAsync(userId);
            if (activeBorrowings == null)
            {
                activeBorrowings = new List<Borrowing>(); 
            }

            if (activeBorrowings.Count >= 3)
            {
                throw new InvalidOperationException("Members can only borrow up to 3 books at a time.");
            }

            int borrowingDays = 14;
            DateTime borrowedAt = DateTime.UtcNow;
            DateTime expectedReturnDate = borrowedAt.AddDays(borrowingDays);

            var borrowing = new Borrowing
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BookId = bookId,
                BookTitle = book.Title, 
                BorrowedAt = borrowedAt,
                ExpectedReturnDate = expectedReturnDate,
                IsOverdue = false
            };

            book.IsAvailable = false;

            await _borrowingRepository.AddBorrowingAsync(borrowing);
            await _bookRepository.UpdateBookAsync(book);

            return MapToBorrowingDTO(borrowing);
        }





        public async Task<List<BorrowingDTO>> GetOverdueBorrowingsAsync()
        {
            var borrowings = await _borrowingRepository.GetAllBorrowingsAsync();
            var overdueBorrowings = borrowings
                .Where(b => b.ExpectedReturnDate < DateTime.UtcNow && b.ReturnedAt == null)
                .Select(MapToBorrowingDTO)
                .ToList();

            return overdueBorrowings;
        }

        public async Task PenalizeOverdueMembersAsync()
        {
            var overdueBorrowings = await GetOverdueBorrowingsAsync();
            foreach (var borrowing in overdueBorrowings)
            {
                // Example penalty: $1 per day overdue
                var daysOverdue = (DateTime.UtcNow - borrowing.ExpectedReturnDate.Value).Days;
                decimal penalty = daysOverdue * 1.0m;

                borrowing.Penalty = penalty;

                // Update the borrowing record
                var borrowingEntity = await _borrowingRepository.GetBorrowingByIdAsync(borrowing.Id);
                borrowingEntity.Penalty = penalty;

                await _borrowingRepository.UpdateBorrowingAsync(borrowingEntity);
            }
        }

        public async Task<List<BorrowingDTO>> GetAllBorrowedBooksAsync()
        {
            var borrowings = await _borrowingRepository.GetAllBorrowingsAsync();

            var overdueBorrowings = borrowings
                .Where(b => b.ExpectedReturnDate < DateTime.UtcNow && b.ReturnedAt == null)
                .Select(MapToBorrowingDTO)
                .ToList();

            return overdueBorrowings;
        }



        public async Task ReturnBookAsync(Guid borrowingId, int ratingValue, string? comment = null)
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

            await AddRatingAsync(borrowing.BookId, borrowing.UserId, ratingValue, comment);
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



        public async Task<List<BorrowingDTO>> GetMemberBorrowedBooksAsync(Guid userId)
        {
            var borrowings = await _borrowingRepository.GetActiveBorrowingsByUserAsync(userId);
            return borrowings.Select(b => MapToBorrowingDTO(b)).ToList();
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

        public async Task<List<BorrowingDTO>> GetBorrowedBooks (Guid userId)
        {
            var borrowings = await _borrowingRepository.GetBorrowedBooks(userId);
            return borrowings.Select(b => MapToBorrowingDTO(b)).ToList();
        }

        public async Task<List<BorrowingDTO>> GetAllBorrowingsAsync()
        {
            var borrowings = await _borrowingRepository.GetAllBorrowingsAsync();

            if (borrowings == null)
            {
                // Log or handle the null case
                return new List<BorrowingDTO>();
            }

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

        private BorrowingDTO MapToBorrowingDTO(Borrowing borrowing)
        {
            return new BorrowingDTO
            {
                Id = borrowing.Id,
                UserId = borrowing.UserId,
                BookId = borrowing.BookId,
                BookTitle = borrowing.Book?.Title,
                BorrowedAt = borrowing.BorrowedAt,
                ExpectedReturnDate = borrowing.ExpectedReturnDate,
                ReturnedAt = borrowing.ReturnedAt,
                IsOverdue = borrowing.ExpectedReturnDate < DateTime.UtcNow && borrowing.ReturnedAt == null,
                Penalty = borrowing.Penalty
            };
        }

    }
}
