namespace LibraryCatalog.Models

{
    public class Book
    {
        public int Id { get; set; }                  // PRIMARY KEY AUTOINCREMENT
        public string Title { get; set; } = null!;   // NOT NULL, max 200
        public string Author { get; set; } = null!;  // NOT NULL
        public int PublishedYear { get; set; }
        public string? Genre { get; set; }           // необязательное
        public bool IsAvailable { get; set; } = true; // по умолчанию true
    }
}
