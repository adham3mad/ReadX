using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadX.Api.DTOs;
using ReadX.Api.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using ReadX.Api.Extensions;

namespace ReadX.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.GetUserId();
        var response = await _userService.GetMyProfileAsync(userId!);
        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var userId = User.GetUserId();
        var response = await _userService.UpdateMyProfileAsync(userId!, request);
        return Ok(response);
    }
}
