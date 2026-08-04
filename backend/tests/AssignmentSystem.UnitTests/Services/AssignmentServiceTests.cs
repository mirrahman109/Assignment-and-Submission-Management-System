using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;

namespace AssignmentSystem.UnitTests.Services;

public class AssignmentServiceTests : IDisposable
{
    private readonly SqliteInMemoryDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly FakeClock _clock;

    public AssignmentServiceTests()
    {
        _factory = new SqliteInMemoryDbContextFactory();
        _db = _factory.Context;
        _clock = new FakeClock();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task CreateAsync_Throws_WhenTeacherNotAssignedToClassSubject()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var service = new AssignmentService(_db, _clock);
        var currentUser = new FakeCurrentUserService(seed.OtherTeacher.Id, RoleType.Teacher);

        var request = new CreateAssignmentRequest(
            "Algebra Basics", "Solve the problems", seed.ClassASubject.Id,
            _clock.UtcNow.AddDays(7), 100, false, PublishImmediately: true);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateAsync(request, currentUser));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WhenTeacherIsAssigned()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var service = new AssignmentService(_db, _clock);
        var currentUser = new FakeCurrentUserService(seed.Teacher.Id, RoleType.Teacher);

        var request = new CreateAssignmentRequest(
            "Algebra Basics", "Solve the problems", seed.ClassASubject.Id,
            _clock.UtcNow.AddDays(7), 100, false, PublishImmediately: true);

        var result = await service.CreateAsync(request, currentUser);

        Assert.Equal("Algebra Basics", result.Title);
        Assert.Equal("Published", result.Status);
        Assert.Equal(seed.Teacher.Id, result.TeacherId);
    }
}
