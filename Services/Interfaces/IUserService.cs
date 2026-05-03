using ReadX.Api.DTOs;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Interfaces;

public interface IUserService
{
    Task<UserResponse> GetMyProfileAsync(string userId);
    Task<UserResponse> UpdateMyProfileAsync(string userId, UpdateProfileRequest request);
}
