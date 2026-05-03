using System;

namespace ReadX.Api.DTOs;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    
    // For profile specifically
    public int? TotalBorrowsCount { get; set; }
    public int? ActiveBorrowsCount { get; set; }
    public int? OverdueBorrowsCount { get; set; }
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirmation { get; set; }
}
