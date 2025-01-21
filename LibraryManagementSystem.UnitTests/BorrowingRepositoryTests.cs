using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Infrastructure.Data;
using FortunaeLibraryManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class BorrowingRepositoryTests
{
    private readonly BorrowingRepository _repository;
    private readonly LibraryDbContext _dbContext;

    public BorrowingRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new LibraryDbContext(options);
        _repository = new BorrowingRepository(_dbContext);
    }

    [Fact]
    public async Task GetActiveBorrowingsByUserAsync_ShouldReturnActiveBorrowings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var borrowing = new Borrowing
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = bookId,
            BorrowedAt = DateTime.UtcNow,
            ReturnedAt = null 
        };

        _dbContext.Borrowings.Add(borrowing);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveBorrowingsByUserAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal(userId, result[0].UserId);
        Assert.Null(result[0].ReturnedAt);
    }
}
