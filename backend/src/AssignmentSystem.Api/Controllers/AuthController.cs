using AssignmentSystem.Api.DTOs.Auth;
using AssignmentSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserSummary>> Me([FromServices] ICurrentUserService currentUser, [FromServices] IUserService userService)
    {
        var user = await userService.GetByIdAsync(currentUser.UserId);
        return Ok(new UserSummary(user.Id, user.FullName, user.Email, user.Role, user.ClassCourseId));
    }
}
