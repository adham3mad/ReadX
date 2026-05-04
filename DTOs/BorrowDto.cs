using System;

namespace ReadX.Api.DTOs;

public class BorrowRequestDto
{
    public string BookId { get; set; } = string.Empty;
    public int RequestedDurationDays { get; set; }
}

public class BorrowResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BookId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookCategory { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int RequestedDurationDays { get; set; }
    public DateTime? BorrowedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int? DaysOverdue { get; set; }
    public int? DaysRemaining { get; set; } // For member "My Borrows"
}

public class BorrowListResponse
{
    public long Total { get; set; }
    public BorrowCounts Counts { get; set; } = new();
    public int Page { get; set; }
    public int Limit { get; set; }
    public System.Collections.Generic.List<BorrowResponse> Borrows { get; set; } = new();
}

public class BorrowCounts
{
    public long Pending { get; set; }
    public long Active { get; set; }
    public long Overdue { get; set; }
}
