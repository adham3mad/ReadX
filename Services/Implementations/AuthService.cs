using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReadX.Api.DTOs;
using ReadX.Api.Exceptions;
using ReadX.Api.Models;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Services.Interfaces;
using ReadX.Api.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ReadX.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IMongoRepository<User> userRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetOneAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new BusinessException("EmailAlreadyRegistered", 409);
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Models.Enums.UserRole.Member
        };

        await _userRepository.CreateAsync(user);

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            User = MapToUserResponse(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetOneAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new BusinessException("InvalidCredentials", 401);
        }

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            User = MapToUserResponse(user)
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString().ToLower()),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLower()) // standard .NET role claim
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString().ToLower(),
            JoinedAt = user.JoinedAt
        };
    }
}
