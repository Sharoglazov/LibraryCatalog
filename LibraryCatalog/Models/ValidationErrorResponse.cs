namespace LibraryCatalog.Models;

public class ValidationErrorResponse
{
    public string Title { get; set; } = "Validation Failed";
    public Dictionary<string, string[]> Errors { get; set; } = new();
}