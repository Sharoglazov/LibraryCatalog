using Dapper;
using Microsoft.Data.Sqlite;
using LibraryCatalog.Models;

namespace LibraryCatalog.Repositories;

public class BookRepository : IBookRepository
{
    private readonly string _connectionString;

    public BookRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task<IEnumerable<Book>> GetAllAsync(string? genre = null, bool? isAvailable = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT * FROM Books WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            sql += " AND Genre = @Genre";
            parameters.Add("@Genre", genre);
        }

        if (isAvailable.HasValue)
        {
            sql += " AND IsAvailable = @IsAvailable";
            parameters.Add("@IsAvailable", isAvailable.Value);
        }

        return await connection.QueryAsync<Book>(sql, parameters);
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        return await connection.QuerySingleOrDefaultAsync<Book>(
            "SELECT * FROM Books WHERE Id = @Id", new { Id = id });
    }

    public async Task<Book> AddAsync(Book book)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"INSERT INTO Books (Title, Author, PublishedYear, Genre, IsAvailable)
                    VALUES (@Title, @Author, @PublishedYear, @Genre, @IsAvailable);
                    SELECT last_insert_rowid();";

        var id = await connection.ExecuteScalarAsync<int>(sql, book);
        book.Id = id;
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"UPDATE Books 
                    SET Title = @Title, Author = @Author, PublishedYear = @PublishedYear,
                        Genre = @Genre, IsAvailable = @IsAvailable
                    WHERE Id = @Id";

        var affected = await connection.ExecuteAsync(sql, book);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var affected = await connection.ExecuteAsync(
            "DELETE FROM Books WHERE Id = @Id", new { Id = id });
        return affected > 0;
    }

    public async Task<bool> UpdateAvailabilityAsync(int id, bool isAvailable)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var affected = await connection.ExecuteAsync(
            "UPDATE Books SET IsAvailable = @IsAvailable WHERE Id = @Id",
            new { Id = id, IsAvailable = isAvailable });
        return affected > 0;
    }
}