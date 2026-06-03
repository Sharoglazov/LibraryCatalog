using LibraryCatalog.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCatalog.Tests.IntegrationTests;

public class BooksApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly string _dbPath;
    private readonly HttpClient _client;

    public BooksApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Абсолютный путь к временной базе
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_books_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={_dbPath}";

        var mainProjectDir = @"C:\VSprojects\LibraryCatalog\LibraryCatalog";

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(mainProjectDir);            // фиксируем корень
            builder.UseSetting("ConnectionStrings:Default", connectionString);
        }).CreateClient();
    }

    [Fact]
    public async Task PostBook_ShouldReturn201AndLocationHeader()
    {
        var newBook = new
        {
            title = "Integration Test Book",
            author = "Author",
            publishedYear = 2023,
            genre = "Test",
            isAvailable = true
        };

        var response = await _client.PostAsJsonAsync("/api/books", newBook);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var createdBook = await response.Content.ReadFromJsonAsync<Book>();
        Assert.NotNull(createdBook);
        Assert.True(createdBook.Id > 0);
        Assert.Equal(newBook.title, createdBook.Title);
    }
}