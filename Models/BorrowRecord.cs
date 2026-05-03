using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadX.Api.Models.Enums;
using System;

namespace ReadX.Api.Models;

public class BorrowRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonRepresentation(BsonType.String)]
    public string UserId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public string BookId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string BookTitle { get; set; } = string.Empty;

    public string BookCategory { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public BorrowStatus Status { get; set; } = BorrowStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? BorrowedAt { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime? ReturnedAt { get; set; }
}
