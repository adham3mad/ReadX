using ReadX.Api.DTOs;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Interfaces;

public interface IBorrowService
{
    Task<BorrowResponse> RequestBorrowAsync(string userId, BorrowRequestDto request);
    Task<BorrowListResponse> GetAllBorrowsAsync(string? status, int page, int limit);
    Task<BorrowResponse> ApproveBorrowAsync(string id);
    Task<BorrowResponse> RejectBorrowAsync(string id);
    Task<BorrowResponse> ReturnBorrowAsync(string id);
    Task<PaginatedResponse<BorrowResponse>> GetMyBorrowsAsync(string userId, string? status, int page, int limit);
}
