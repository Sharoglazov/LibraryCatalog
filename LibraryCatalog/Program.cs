using LibraryCatalog.Repositories;
using LibraryCatalog.Endpoints;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Default")?? "Data Source=data/books.db";

builder.Services.AddScoped<IBookRepository>(_ => new BookRepository(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

EnsureDatabaseCreated(connectionString);

app.UseSwagger();
app.UseSwaggerUI();

app.MapBookEndpoints();

app.Run();

void EnsureDatabaseCreated(string connString)
{
    var csb = new SqliteConnectionStringBuilder(connString);
    string dbPath = csb.DataSource;
    string? dataFolder = Path.GetDirectoryName(dbPath);
    if (!string.IsNullOrEmpty(dataFolder) && !Directory.Exists(dataFolder))
        Directory.CreateDirectory(dataFolder);

    using var connection = new SqliteConnection(connString);
    connection.Open();
    var createTableSql = @"
        CREATE TABLE IF NOT EXISTS Books (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Author TEXT NOT NULL,
            PublishedYear INTEGER NOT NULL,
            Genre TEXT,
            IsAvailable INTEGER NOT NULL DEFAULT 1
        )";
    using var command = connection.CreateCommand();
    command.CommandText = createTableSql;
    command.ExecuteNonQuery();
}

public partial class Program { }