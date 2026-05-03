using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ReadX.Api.Models;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Isbn { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }

    public string? Description { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }
}
