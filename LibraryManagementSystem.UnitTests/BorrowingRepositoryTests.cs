
namespace LibraryManagementSystem.UnitTests
{
    using FortunaeLibraryManagementSystem.Domain.Entities;
    using FortunaeLibraryManagementSystem.Infrastructure.Data;
    using FortunaeLibraryManagementSystem.Infrastructure.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class BorrowingRepositoryTests : IDisposable
    {
        private readonly LibraryDbContext _dbContext;
        private readonly BorrowingRepository _repository;
        private readonly Guid _userId;
        private readonly Guid _bookId;
        private readonly Guid _borrowingId;

        public BorrowingRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LibraryDbContext(options);
            _repository = new BorrowingRepository(_dbContext);

            _userId = Guid.NewGuid();
            _bookId = Guid.NewGuid();
            _borrowingId = Guid.NewGuid();

            SeedUserAndBook().Wait();
        }

        private async Task SeedUserAndBook()
        {
            var user = new User { Id = _userId, Name = "Test User", Email = "test@example.com" };
            var book = new Book { Id = _bookId, Title = "Test Book", Author = "Test Author" };

            _dbContext.Users.Add(user);
            _dbContext.Books.Add(book);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SetupTestData()
        {
            var borrowing = new Borrowing
            {
                Id = _borrowingId,
                UserId = _userId,
                BookId = _bookId,
                BorrowedAt = DateTime.UtcNow.AddDays(-10),
                ExpectedReturnDate = DateTime.UtcNow.AddDays(-5),
                ReturnedAt = null
            };

            await _repository.AddBorrowingAsync(borrowing);
        }

        [Fact]
        public async Task AddBorrowingAsync_ShouldAddNewBorrowing()
        {
            // Arrange
            var borrowing = new Borrowing
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                BookId = _bookId,
                BorrowedAt = DateTime.UtcNow,
                ExpectedReturnDate = DateTime.UtcNow.AddDays(14),
                ReturnedAt = null
            };

            // Act
            await _repository.AddBorrowingAsync(borrowing);

            var allBorrowings = await _dbContext.Borrowings.ToListAsync();
            Console.WriteLine($"Total Borrowings in DB: {allBorrowings.Count}");

            // Assert
            var addedBorrowing = await _repository.GetBorrowingByIdAsync(borrowing.Id);
            Assert.NotNull(addedBorrowing);
            Assert.Equal(_userId, addedBorrowing.UserId);
            Assert.Equal(_bookId, addedBorrowing.BookId);
            Assert.Null(addedBorrowing.ReturnedAt);
        }

        [Fact]
        public async Task GetActiveBorrowingsByUserAsync_ShouldReturnActiveBorrowings()
        {
            // Arrange
            await SetupTestData();

            // Act
            var result = await _repository.GetActiveBorrowingsByUserAsync(_userId);

            // Debugging
            Console.WriteLine($"Active borrowings found: {result.Count}");

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, b => Assert.Null(b.ReturnedAt));
        }

        [Fact]
        public async Task GetBorrowingHistoryByUserAsync_ShouldReturnOrderedHistory()
        {
            // Arrange
            await SetupTestData();

            var oldBorrowing = new Borrowing
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                BookId = _bookId,
                BorrowedAt = DateTime.UtcNow.AddDays(-50),
                ExpectedReturnDate = DateTime.UtcNow.AddDays(-30),
                ReturnedAt = DateTime.UtcNow.AddDays(-40)
            };

            await _repository.AddBorrowingAsync(oldBorrowing);

            // Act
            var result = await _repository.GetBorrowingHistoryByUserAsync(_userId);

            // Debugging
            Console.WriteLine($"Borrowing history count: {result.Count}");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result[0].BorrowedAt > result[1].BorrowedAt);
        }

        [Fact]
        public async Task GetAllBorrowingsAsync_ShouldReturnAllBorrowingsWithIncludes()
        {
            // Arrange
            await SetupTestData();

            // Act
            var result = await _repository.GetAllBorrowingsAsync();

            // Debugging
            Console.WriteLine($"Total borrowings retrieved: {result.Count}");

            // Assert
            Assert.Single(result);
            Assert.NotNull(result[0].Book);
            Assert.NotNull(result[0].User);
        }

        [Fact]
        public async Task GetOverdueBorrowingsAsync_ShouldReturnOverdueBorrowings()
        {
            // Arrange
            await SetupTestData();

            // Act
            var result = await _repository.GetOverdueBorrowingsAsync();

            // Debugging
            Console.WriteLine($"Overdue borrowings found: {result.Count}");

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, b => Assert.True(b.ExpectedReturnDate < DateTime.UtcNow));
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
