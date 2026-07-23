using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using WMS.API.Infrastructure;

namespace WMS.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetUsers([FromQuery] string? keyword, [FromQuery] bool? isActive,
        [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _userService.GetUsersAsync(keyword, isActive, pageIndex, pageSize);
        return Ok(PagedResponse<object>.From(
            items.Select(u => new { u.Id, u.Username, u.Email, u.FullName, u.Phone, u.IsActive, u.CreatedAt }).ToList<object>(),
            totalCount, pageIndex, pageSize));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse<object>.Ok(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.GetUserId();
        var user = await _userService.GetMeAsync(userId);
        return Ok(ApiResponse<object>.Ok(user));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return StatusCode(201, ApiResponse<object>.Created(new { user.Id, user.Username, user.Email }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(new { user.Id, user.FullName }));
    }

    [HttpPatch("{id}/toggle-activate")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ToggleActivate(int id)
    {
        await _userService.ToggleActivateAsync(id);
        return Ok(ApiResponse.OkMessage("User activation toggled"));
    }

    [HttpPost("{id}/roles")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] List<int> roleIds)
    {
        await _userService.AssignRolesAsync(id, roleIds);
        return Ok(ApiResponse.OkMessage("Roles assigned"));
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.GetUserId();
        await _userService.UpdateProfileAsync(userId, dto);
        return Ok(ApiResponse.OkMessage("Profile updated"));
    }

    [HttpPatch("me/language")]
    public async Task<IActionResult> ChangeLanguage([FromBody] ChangeLanguageRequest request)
    {
        var userId = User.GetUserId();
        await _userService.ChangeLanguageAsync(userId, request.Lang);
        return Ok(ApiResponse.OkMessage("Language updated"));
    }
}

public record ChangeLanguageRequest(string Lang);
