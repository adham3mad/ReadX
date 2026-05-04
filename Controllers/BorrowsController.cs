using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadX.Api.DTOs;
using ReadX.Api.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using ReadX.Api.Extensions;

namespace ReadX.Api.Controllers;

[ApiController]
[Route("borrows")]
[Authorize]
public class BorrowsController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowsController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpPost]
    [Authorize(Policy = "MemberOnly")]
    public async Task<IActionResult> RequestBorrow(BorrowRequestDto request)
    {
        var userId = User.GetUserId();
        var response = await _borrowService.RequestBorrowAsync(userId!, request);
        return Created($"/borrows/{response.Id}", response);
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllBorrows([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var response = await _borrowService.GetAllBorrowsAsync(status, page, limit);
        return Ok(response);
    }

    [HttpPatch("{id}/approve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ApproveBorrow(string id)
    {
        var response = await _borrowService.ApproveBorrowAsync(id);
        return Ok(response);
    }

    [HttpPatch("{id}/reject")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RejectBorrow(string id)
    {
        var response = await _borrowService.RejectBorrowAsync(id);
        return Ok(response);
    }

    [HttpPatch("{id}/return")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ReturnBorrow(string id)
    {
        var response = await _borrowService.ReturnBorrowAsync(id);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<IActionResult> GetMyBorrows([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var userId = User.GetUserId();
        var response = await _borrowService.GetMyBorrowsAsync(userId!, status, page, limit);
        return Ok(response);
    }
}
