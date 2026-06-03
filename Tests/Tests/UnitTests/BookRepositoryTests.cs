using Microsoft.Data.Sqlite;
using LibraryCatalog.Models;
using LibraryCatalog.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCatalog.Tests.UnitTests;

public class BookRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private BookRepository _repository = null!;

    // Строка подключения к SQLite в памяти
    private const string ConnectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared";

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection(ConnectionString);
        await _connection.OpenAsync();

        // Создание таблицы
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE Books (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Author TEXT NOT NULL,
                PublishedYear INTEGER NOT NULL,
                Genre TEXT,
                IsAvailable INTEGER NOT NULL DEFAULT 1
            )";
        await command.ExecuteNonQueryAsync();

        _repository = new BookRepository(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldInsertBookAndReturnWithId()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Title",
            Author = "Test Author",
            PublishedYear = 2020,
            Genre = "Fiction",
            IsAvailable = true
        };

        // Act
        var created = await _repository.AddAsync(book);

        // Assert
        Assert.True(created.Id > 0);
        Assert.Equal("Test Title", created.Title);
        Assert.Equal("Test Author", created.Author);
    }

    [Fact]
    public async Task GetAllAsync_WithGenreFilter_ShouldReturnFilteredBooks()
    {
        // Arrange
        await _repository.AddAsync(new Book { Title = "Book 1", Author = "A", PublishedYear = 2000, Genre = "Fantasy" });
        await _repository.AddAsync(new Book { Title = "Book 2", Author = "B", PublishedYear = 2001, Genre = "Sci-Fi" });

        // Act
        var books = await _repository.GetAllAsync(genre: "Fantasy");

        // Assert
        Assert.Single(books);
        Assert.Equal("Fantasy", books.First().Genre);
    }

    [Fact]
    public async Task UpdateAvailabilityAsync_ShouldToggleFlag()
    {
        // Arrange
        var book = await _repository.AddAsync(new Book
        {
            Title = "To Update",
            Author = "Someone",
            PublishedYear = 1999,
            IsAvailable = true
        });

        // Act
        var result = await _repository.UpdateAvailabilityAsync(book.Id, false);

        // Assert
        Assert.True(result);
        var updated = await _repository.GetByIdAsync(book.Id);
        Assert.NotNull(updated);
        Assert.False(updated!.IsAvailable);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBook()
    {
        // Arrange
        var book = await _repository.AddAsync(new Book
        {
            Title = "Delete Me",
            Author = "Ghost",
            PublishedYear = 2005
        });

        // Act
        var deleted = await _repository.DeleteAsync(book.Id);

        // Assert
        Assert.True(deleted);
        var notFound = await _repository.GetByIdAsync(book.Id);
        Assert.Null(notFound);
    }
}