using System;

namespace ReadX.Api.DTOs;

public class BookResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int? BorrowedCopies { get; set; } // for Get Book detail
}

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
    public int TotalCopies { get; set; }
}

public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public string? Category { get; set; }
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
    public int? TotalCopies { get; set; }
}

public class CategoryResponse
{
    public System.Collections.Generic.List<string> Categories { get; set; } = new();
}
