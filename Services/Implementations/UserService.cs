using ReadX.Api.DTOs;
using ReadX.Api.Exceptions;
using ReadX.Api.Models;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Services.Interfaces;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Implementations;

public class UserService : IUserService
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IMongoRepository<BorrowRecord> _borrowRepository;

    public UserService(IMongoRepository<User> userRepository, IMongoRepository<BorrowRecord> borrowRepository)
    {
        _userRepository = userRepository;
        _borrowRepository = borrowRepository;
    }

    public async Task<UserResponse> GetMyProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new BusinessException("UserNotFound", 404);

        var response = new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString().ToLower(),
            JoinedAt = user.JoinedAt,
            TotalBorrowsCount = (int)await _borrowRepository.CountAsync(b => b.UserId == userId),
            ActiveBorrowsCount = (int)await _borrowRepository.CountAsync(b => b.UserId == userId && b.Status == Models.Enums.BorrowStatus.Active),
            OverdueBorrowsCount = (int)await _borrowRepository.CountAsync(b => b.UserId == userId && b.Status == Models.Enums.BorrowStatus.Overdue)
        };

        return response;
    }

    public async Task<UserResponse> UpdateMyProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new BusinessException("UserNotFound", 404);

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        
        if (!string.IsNullOrEmpty(request.Password))
        {
            if (request.Password != request.PasswordConfirmation)
            {
                throw new BusinessException("PasswordsDoNotMatch", 422);
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(userId, user);
        
        return await GetMyProfileAsync(userId);
    }
}
