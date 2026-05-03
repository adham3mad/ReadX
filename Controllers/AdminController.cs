using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadX.Api.Services.Interfaces;
using System.Threading.Tasks;

namespace ReadX.Api.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var response = await _adminService.GetStatsAsync();
        return Ok(response);
    }

    [HttpGet("pending-requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var response = await _adminService.GetPendingRequestsAsync();
        return Ok(response);
    }
}
