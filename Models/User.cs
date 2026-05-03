using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadX.Api.Models.Enums;
using System;

namespace ReadX.Api.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; } = UserRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
