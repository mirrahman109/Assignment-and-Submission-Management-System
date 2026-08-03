using AssignmentSystem.Api.DTOs.Users;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> List([FromQuery] string? role)
    {
        RoleType? roleFilter = null;
        if (!string.IsNullOrEmpty(role) && Enum.TryParse<RoleType>(role, true, out var parsed))
        {
            roleFilter = parsed;
        }
        return Ok(await _userService.ListAsync(roleFilter));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id) => Ok(await _userService.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request) =>
        Ok(await _userService.UpdateAsync(id, request));

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _userService.DeactivateAsync(id);
        return NoContent();
    }
}
