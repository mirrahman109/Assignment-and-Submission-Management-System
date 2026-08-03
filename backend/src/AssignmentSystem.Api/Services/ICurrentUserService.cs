using System.Security.Claims;
using AssignmentSystem.Api.Models.Entities.Enums;

namespace AssignmentSystem.Api.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    RoleType Role { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return value is null ? 0 : int.Parse(value);
        }
    }

    public RoleType Role
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return value is null ? default : Enum.Parse<RoleType>(value);
        }
    }
}
