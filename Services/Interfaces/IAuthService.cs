using ReadX.Api.DTOs;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
