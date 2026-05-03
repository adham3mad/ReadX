using ReadX.Api.DTOs;
using ReadX.Api.Exceptions;
using ReadX.Api.Models;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IMongoRepository<Book> _bookRepository;
    private readonly IMongoRepository<BorrowRecord> _borrowRepository;

    public AdminService(IMongoRepository<User> userRepository, IMongoRepository<Book> bookRepository, IMongoRepository<BorrowRecord> borrowRepository)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _borrowRepository = borrowRepository;
    }

    public async Task<StatsResponse> GetStatsAsync()
    {
        return new StatsResponse
        {
            TotalBooks = await _bookRepository.CountAsync(b => true),
            TotalMembers = await _userRepository.CountAsync(u => u.Role == Models.Enums.UserRole.Member),
            ActiveBorrowsCount = await _borrowRepository.CountAsync(b => b.Status == Models.Enums.BorrowStatus.Active),
            OverdueBorrowsCount = await _borrowRepository.CountAsync(b => b.Status == Models.Enums.BorrowStatus.Overdue)
        };
    }

    public async Task<AdminPendingRequestsResponse> GetPendingRequestsAsync()
    {
        var filter = MongoDB.Driver.Builders<BorrowRecord>.Filter.Eq(b => b.Status, Models.Enums.BorrowStatus.Pending);
        // Getting top 5 latest
        var pending = await _borrowRepository.GetAllAsync(filter, 1, 5);
        pending = pending.OrderByDescending(p => p.RequestedAt).ToList();

        return new AdminPendingRequestsResponse
        {
            PendingRequests = pending.Select(p => new PendingRequestResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.UserName,
                BookTitle = p.BookTitle,
                RequestedAt = p.RequestedAt
            }).ToList()
        };
    }

    public async Task<PaginatedResponse<UserResponse>> GetMembersAsync(string? query, int page, int limit)
    {
        var filter = MongoDB.Driver.Builders<User>.Filter.Eq(u => u.Role, Models.Enums.UserRole.Member);
        
        if (!string.IsNullOrEmpty(query))
        {
            var queryFilter = MongoDB.Driver.Builders<User>.Filter.Or(
                MongoDB.Driver.Builders<User>.Filter.Regex(u => u.FullName, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                MongoDB.Driver.Builders<User>.Filter.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            );
            filter = MongoDB.Driver.Builders<User>.Filter.And(filter, queryFilter);
        }

        var members = await _userRepository.GetAllAsync(filter, page, limit);
        var total = await _userRepository.CountAsync(filter);

        var data = members.Select(u => new UserResponse
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Phone = u.Phone,
            JoinedAt = u.JoinedAt
        }).ToList();

        // Count active/overdue borrows for each member
        foreach (var member in data)
        {
            member.ActiveBorrowsCount = (int)await _borrowRepository.CountAsync(b => b.UserId == member.Id && b.Status == Models.Enums.BorrowStatus.Active);
            member.OverdueBorrowsCount = (int)await _borrowRepository.CountAsync(b => b.UserId == member.Id && b.Status == Models.Enums.BorrowStatus.Overdue);
        }

        return new PaginatedResponse<UserResponse>
        {
            Total = total,
            Page = page,
            Limit = limit,
            Data = data
        };
    }

    public async Task DeleteMemberAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id) ?? throw new BusinessException("UserNotFound", 404);

        var hasActiveBorrows = await _borrowRepository.CountAsync(b => b.UserId == id && (b.Status == Models.Enums.BorrowStatus.Active || b.Status == Models.Enums.BorrowStatus.Pending));
        if (hasActiveBorrows > 0)
        {
            throw new BusinessException("MemberHasActiveBorrows", 409);
        }

        await _userRepository.RemoveAsync(id);
    }
}
