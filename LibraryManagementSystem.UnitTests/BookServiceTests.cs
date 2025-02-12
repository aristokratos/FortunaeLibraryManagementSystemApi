using Xunit;
using Moq;
using FortunaeLibraryManagementSystem.Service.Services;
using FortunaeLibraryManagementSystem.Service.Interfaces;
using FortunaeLibraryManagementSystem.Service.DTOs;
using FortunaeLibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FortunaeLibraryManagementSystem.Infrastructure.Interfaces;
using FortunaeLibraryManagementSystem.Service.Services.CacheService;
using System.IO;
using Microsoft.AspNetCore.Http;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IRatingRepository> _ratingRepositoryMock;
    private readonly Mock<IImageService> _imageServiceMock;
    private readonly Mock<IRedisService> _cacheMock;
    private readonly Mock<ILogger<BookService>> _loggerMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _ratingRepositoryMock = new Mock<IRatingRepository>();
        _imageServiceMock = new Mock<IImageService>();
        _cacheMock = new Mock<IRedisService>();
        _loggerMock = new Mock<ILogger<BookService>>();

        _bookService = new BookService(
            _bookRepositoryMock.Object,
            _loggerMock.Object,
            _imageServiceMock.Object,
            _cacheMock.Object,
            _ratingRepositoryMock.Object
        );
    }

    [Fact]
    public async Task AddBookAsync_ShouldAddBook_WhenValidInputProvided()
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(new byte[0]);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Arrange
        var createBookDto = new CreateBookDTO
        {
            Title = "Test Book",
            Author = "Test Author",
            Genre = "Fiction",
            ISBN = "1234567890",
            Description = "A test book description",
            Image = fileMock.Object,

        };

        var imageResponse = new ImageUrlResponseDto { PresignedUrl = "http://image.url" };
        _imageServiceMock.Setup(x => x.UploadImageAsync(fileMock.Object)).ReturnsAsync(imageResponse);

        _bookRepositoryMock.Setup(x => x.AddBookAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        // Act
        var result = await _bookService.AddBookAsync(createBookDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(createBookDto.Title);
        result.BookImage.Should().Be(imageResponse.PresignedUrl);
    }

    [Fact]
    public async Task GetBooksByIdAsync_ShouldReturnBook_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id = bookId,
            Title = "Sample Book",
            Author = "Author",
            Genre = "Genre",
            ISBN = "123456",
            Description = "Description",
            IsAvailable = true
        };

        _bookRepositoryMock.Setup(x => x.GetBookByIdAsync(bookId)).ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBooksByIdAsync(bookId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(bookId);
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldReturnFalse_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(x => x.GetBookByIdAsync(bookId)).ReturnsAsync((Book)null);

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldReturnTrue_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, Title = "Sample Book" };
        _bookRepositoryMock.Setup(x => x.GetBookByIdAsync(bookId)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(x => x.DeleteBookAsync(book)).Returns(Task.CompletedTask);

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.Should().BeTrue();
    }
}
