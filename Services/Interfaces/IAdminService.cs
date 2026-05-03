using ReadX.Api.DTOs;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Interfaces;

public interface IAdminService
{
    Task<StatsResponse> GetStatsAsync();
    Task<AdminPendingRequestsResponse> GetPendingRequestsAsync();
    Task<PaginatedResponse<UserResponse>> GetMembersAsync(string? query, int page, int limit);
    Task DeleteMemberAsync(string id);
}
