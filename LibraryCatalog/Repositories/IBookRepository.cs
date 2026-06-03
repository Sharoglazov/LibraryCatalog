using LibraryCatalog.Models;

namespace LibraryCatalog.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync(string? genre = null, bool? isAvailable = null);
    Task<Book?> GetByIdAsync(int id);
    Task<Book> AddAsync(Book book);               // возвращает книгу с присвоенным Id
    Task<bool> UpdateAsync(Book book);            // true – обновлена, false – не найдена
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAvailabilityAsync(int id, bool isAvailable);
}
