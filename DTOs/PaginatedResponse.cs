using System.Collections.Generic;

namespace ReadX.Api.DTOs;

public class PaginatedResponse<T>
{
    public long Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public List<T> Data { get; set; } = new();
}
