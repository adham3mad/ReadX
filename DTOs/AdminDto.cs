using System;

namespace ReadX.Api.DTOs;

public class StatsResponse
{
    public long TotalBooks { get; set; }
    public long TotalMembers { get; set; }
    public long ActiveBorrowsCount { get; set; }
    public long OverdueBorrowsCount { get; set; }
}

public class PendingRequestResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}

public class AdminPendingRequestsResponse
{
    public System.Collections.Generic.List<PendingRequestResponse> PendingRequests { get; set; } = new();
}
