using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadX.Api.Services.Interfaces;
using System.Threading.Tasks;

namespace ReadX.Api.Controllers;

[ApiController]
[Route("members")]
[Authorize(Policy = "AdminOnly")]
public class MembersController : ControllerBase
{
    private readonly IAdminService _adminService;

    public MembersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var response = await _adminService.GetMembersAsync(q, page, limit);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(string id)
    {
        await _adminService.DeleteMemberAsync(id);
        return NoContent();
    }
}
