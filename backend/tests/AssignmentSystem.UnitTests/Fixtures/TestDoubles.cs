using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;

namespace AssignmentSystem.UnitTests.Fixtures;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}

public class FakeCurrentUserService : ICurrentUserService
{
    public int UserId { get; set; }
    public RoleType Role { get; set; }

    public FakeCurrentUserService(int userId, RoleType role)
    {
        UserId = userId;
        Role = role;
    }
}
