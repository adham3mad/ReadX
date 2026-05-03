using ReadX.Api.DTOs;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Interfaces;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetBooksAsync(string? query, string? category, int page, int limit);
    Task<BookResponse> GetBookAsync(string id);
    Task<BookResponse> CreateBookAsync(CreateBookRequest request);
    Task<BookResponse> UpdateBookAsync(string id, UpdateBookRequest request);
    Task DeleteBookAsync(string id);
    Task<CategoryResponse> GetCategoriesAsync();
}
