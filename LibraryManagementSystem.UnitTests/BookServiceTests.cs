using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Service.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace LibraryManagementSystem.UnitTests
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<IDistributedCache> _cacheMock; // Mock for Redis Cache
        private readonly IBookService _bookService;

        public BookServiceTests()
        {
            // Initialize Mocks
            _bookRepositoryMock = new Mock<IBookRepository>();
            _cacheMock = new Mock<IDistributedCache>();

            // Inject the Mocks into the Service
            _bookService = new BookService(_bookRepositoryMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnBooks_FromCache_WhenCacheIsAvailable()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", IsAvailable = true },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", IsAvailable = true }
            };
            var cachedBooks = JsonSerializer.Serialize(books);

            // Mock Redis cache to return cached data
            _cacheMock
                .Setup(cache => cache.GetStringAsync("AllBooks", default))
                .ReturnsAsync(cachedBooks);

            // Act
            var result = await _bookService.GetBooksAsync(true, null, null, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].Title.Should().Be("Book 1");
            result[1].Title.Should().Be("Book 2");

            // Ensure the repository is NOT called (since data came from cache)
            _bookRepositoryMock.Verify(repo => repo.GetBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            // Ensure cache was accessed
            _cacheMock.Verify(cache => cache.GetStringAsync("AllBooks", default), Times.Once);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnBooks_FromDatabase_WhenCacheIsNotAvailable()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", IsAvailable = true },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", IsAvailable = true }
            };

            // Mock Redis cache to return null (cache miss)
            _cacheMock
                .Setup(cache => cache.GetStringAsync("AllBooks", default))
                .ReturnsAsync((string)null);

            // Mock repository to return books from the database
            _bookRepositoryMock
                .Setup(repo => repo.GetBooksAsync(null, null, 1, 10))
                .ReturnsAsync(books);

            // Act
            var result = await _bookService.GetBooksAsync(true, null, null, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].Title.Should().Be("Book 1");
            result[1].Title.Should().Be("Book 2");

            // Ensure the repository was called (since data came from the database)
            _bookRepositoryMock.Verify(repo => repo.GetBooksAsync(null, null, 1, 10), Times.Once);

            // Ensure cache was accessed
            _cacheMock.Verify(cache => cache.GetStringAsync("AllBooks", default), Times.Once);

            // Ensure cache was updated
            _cacheMock.Verify(cache => cache.SetStringAsync(
                "AllBooks", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnFilteredBooks_WhenFilterIsApplied()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", Genre = "Fiction", IsAvailable = true },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", Genre = "Non-Fiction", IsAvailable = true }
            };

            // Mock Redis cache to return null (cache miss)
            _cacheMock
                .Setup(cache => cache.GetStringAsync("AllBooks", default))
                .ReturnsAsync((string)null);

            // Mock repository to return books matching the filter
            _bookRepositoryMock
                .Setup(repo => repo.GetBooksAsync("Fiction", null, 1, 10))
                .ReturnsAsync(books.FindAll(book => book.Genre == "Fiction"));

            // Act
            var result = await _bookService.GetBooksAsync(true, "Fiction", null, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Title.Should().Be("Book 1");

            // Ensure the repository was called with the filter
            _bookRepositoryMock.Verify(repo => repo.GetBooksAsync("Fiction", null, 1, 10), Times.Once);

            // Ensure cache was accessed
            _cacheMock.Verify(cache => cache.GetStringAsync("AllBooks", default), Times.Once);

            // Ensure cache was updated
            _cacheMock.Verify(cache => cache.SetStringAsync(
                "AllBooks", It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }
    }
}
