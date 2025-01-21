using FortunaeLibraryManagementSystem.Domain.Entities;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Service.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;

namespace LibraryManagementSystem.UnitTests
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly IBookService _bookService;

        public BookServiceTests()
        {
            // Initialize Mock Repository
            _bookRepositoryMock = new Mock<IBookRepository>();

            // Inject the Mock into the Service
            _bookService = new BookService(_bookRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnBooks_WhenCalled()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", IsAvailable = true },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", IsAvailable = true }
            };

            // Mock the repository method to return the books
            _bookRepositoryMock
                .Setup(repo => repo.GetBooksAsync(null, null, 1, 10))
                .ReturnsAsync(books);

            // Act
            var result = await _bookService.GetAllBooksAsync(true);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].Title.Should().Be("Book 1");
            result[1].Title.Should().Be("Book 2");

            // Verify the repository method was called once
            _bookRepositoryMock.Verify(repo => repo.GetBooksAsync(null, null, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnEmptyList_WhenNoBooksAvailable()
        {
            // Arrange
            _bookRepositoryMock
                .Setup(repo => repo.GetBooksAsync(null, null, 1, 10))
                .ReturnsAsync(new List<Book>()); // Return an empty list

            // Act
            var result = await _bookService.GetAllBooksAsync(true);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(0);

            // Verify the repository method was called once
            _bookRepositoryMock.Verify(repo => repo.GetBooksAsync(null, null, 1, 10), Times.Once);
        }
    }
}
