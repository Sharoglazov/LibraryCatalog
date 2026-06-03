using Microsoft.AspNetCore.Http.HttpResults;
using LibraryCatalog.Models;
using LibraryCatalog.Repositories;
using System.ComponentModel.DataAnnotations;

namespace LibraryCatalog.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books");

        // GET /api/books
        group.MapGet("/", GetAllBooks)
            .WithName("GetAllBooks");

        // GET /api/books/{id}
        group.MapGet("/{id:int}", GetBookById)
            .WithName("GetBookById");

        // POST /api/books
        group.MapPost("/", CreateBook)
            .WithName("CreateBook");

        // PUT /api/books/{id}
        group.MapPut("/{id:int}", UpdateBook)
            .WithName("UpdateBook");

        // DELETE /api/books/{id}
        group.MapDelete("/{id:int}", DeleteBook)
            .WithName("DeleteBook");

        // PATCH /api/books/{id}/availability
        group.MapPatch("/{id:int}/availability", UpdateAvailability)
            .WithName("UpdateAvailability");
    }

    // Обработчики

    private static async Task<Ok<List<Book>>> GetAllBooks(IBookRepository repo, string? genre = null, bool? isAvailable = null)
    {
        var books = await repo.GetAllAsync(genre, isAvailable);
        return TypedResults.Ok(books.ToList());
    }

    private static async Task<Results<Ok<Book>, NotFound>> GetBookById(IBookRepository repo, int id)
    {
        var book = await repo.GetByIdAsync(id);
        if (book is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(book);
    }

    private static async Task<Results<Created<Book>, BadRequest<ValidationErrorResponse>>> CreateBook(IBookRepository repo, Book book)
    {
        // Валидация
        var errors = ValidateBook(book);
        if (errors.Count > 0)
        {
            return TypedResults.BadRequest(new ValidationErrorResponse
            {
                Errors = errors
            });
        }

        var created = await repo.AddAsync(book);
        return TypedResults.Created($"/api/books/{created.Id}", created);
    }

    private static async Task<Results<NoContent, BadRequest<ValidationErrorResponse>, NotFound>> UpdateBook(IBookRepository repo, int id, Book book)
    {
        // Валидация
        var errors = ValidateBook(book);
        if (errors.Count > 0)
        {
            return TypedResults.BadRequest(new ValidationErrorResponse
            {
                Errors = errors
            });
        }

        // Проверка существования книги с таким id
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        book.Id = id; // Принудительно устанавливаем Id из маршрута
        var updated = await repo.UpdateAsync(book);
        if (!updated)
            return TypedResults.NotFound(); // На случай редкой ситуации

        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteBook(IBookRepository repo, int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted)
            return TypedResults.NotFound();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, BadRequest<string>, NotFound>> UpdateAvailability(IBookRepository repo, int id,
    AvailabilityUpdate request)
    {
        // Простейшая валидация
        if (request is null)
            return TypedResults.BadRequest("Request body is required.");

        var updated = await repo.UpdateAvailabilityAsync(id, request.IsAvailable);
        if (!updated)
            return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

    // DTO для PATCH-запроса
    public record AvailabilityUpdate(bool IsAvailable);

    private static Dictionary<string, string[]> ValidateBook(Book book)
    {
        var errors = new Dictionary<string, string[]>();

        // Title обязателен и длина < 200
        if (string.IsNullOrWhiteSpace(book.Title))
            errors["Title"] = new[] { "Title is required." };
        else if (book.Title.Length > 200)
            errors["Title"] = new[] { "Title must be at most 200 characters." };

        // Author обязателен
        if (string.IsNullOrWhiteSpace(book.Author))
            errors["Author"] = new[] { "Author is required." };

        // PublishedYear диапазон от 1450 до текущего года
        int currentYear = DateTime.Now.Year;
        if (book.PublishedYear < 1450 || book.PublishedYear > currentYear)
            errors["PublishedYear"] = new[] { $"PublishedYear must be between 1450 and {currentYear}." };

        // Genre необязательный, валидация не требуется

        // IsAvailable – по умолчанию true, но можно принять любое bool, не валидируем

        return errors;
    }
}