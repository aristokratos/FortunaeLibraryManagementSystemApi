using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FortunaeLibraryManagementSystem.Service.Services;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Domain.Constants;


namespace LibraryManagementSystem.UnitTests;

public class BorrowingServiceTests
{
    private readonly Mock<IBorrowingRepository> _mockBorrowingRepository;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IRatingRepository> _mockRatingRepository;
    private readonly BorrowingService _borrowingService;

    public BorrowingServiceTests()
    {
        _mockBorrowingRepository = new Mock<IBorrowingRepository>();
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRatingRepository = new Mock<IRatingRepository>();

        _borrowingService = new BorrowingService(
            _mockBorrowingRepository.Object,
            _mockBookRepository.Object,
            _mockRatingRepository.Object
        );
    }

    [Fact]
    public async Task BorrowBookAsync_ShouldBorrowBook_WhenConditionsAreMet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            IsAvailable = true
        };

        var activeBorrowings = new List<Borrowing>(); // User has no active borrowings

        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBorrowingRepository
            .Setup(repo => repo.GetActiveBorrowingsByUserAsync(userId))
            .ReturnsAsync(activeBorrowings);

        _mockBorrowingRepository
            .Setup(repo => repo.AddBorrowingAsync(It.IsAny<Borrowing>()))
            .Returns(Task.CompletedTask);

        _mockBookRepository
            .Setup(repo => repo.UpdateBookAsync(It.IsAny<Book>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _borrowingService.BorrowBookAsync(userId, bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookId, result.BookId);
        Assert.Equal(userId, result.UserId);
        Assert.False(book.IsAvailable); // Book should be marked as unavailable

        // Verify that AddBorrowingAsync was called once
        _mockBorrowingRepository.Verify(repo => repo.AddBorrowingAsync(It.IsAny<Borrowing>()), Times.Once);

        // Verify that UpdateBookAsync was called once
        _mockBookRepository.Verify(repo => repo.UpdateBookAsync(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task BorrowBookAsync_ShouldThrowException_WhenBookDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(bookId))
            .ReturnsAsync((Book)null); // Book not found

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _borrowingService.BorrowBookAsync(userId, bookId)
        );

        Assert.Equal(Message.BookDoesNotExist, exception.Message);
    }

    [Fact]
    public async Task BorrowBookAsync_ShouldThrowException_WhenBookIsUnavailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            IsAvailable = false // Book is already borrowed
        };

        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _borrowingService.BorrowBookAsync(userId, bookId)
        );

        Assert.Equal(Message.BookNotAvailable, exception.Message);
    }

    [Fact]
    public async Task BorrowBookAsync_ShouldThrowException_WhenUserHasMaxBorrowings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            IsAvailable = true
        };

        var activeBorrowings = new List<Borrowing>
        {
            new Borrowing(), new Borrowing(), new Borrowing() // User has 3 active borrowings
        };

        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBorrowingRepository
            .Setup(repo => repo.GetActiveBorrowingsByUserAsync(userId))
            .ReturnsAsync(activeBorrowings);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _borrowingService.BorrowBookAsync(userId, bookId)
        );

        Assert.Equal(Message.BorrowLimimit, exception.Message);
    }
}
