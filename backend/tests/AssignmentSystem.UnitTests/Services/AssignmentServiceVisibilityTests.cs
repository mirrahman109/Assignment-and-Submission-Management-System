using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;

namespace AssignmentSystem.UnitTests.Services;

public class AssignmentServiceVisibilityTests : IDisposable
{
    private readonly SqliteInMemoryDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly FakeClock _clock;

    public AssignmentServiceVisibilityTests()
    {
        _factory = new SqliteInMemoryDbContextFactory();
        _db = _factory.Context;
        _clock = new FakeClock();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetByIdAsync_Throws_NotFound_WhenAssignmentIsStillDraft()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var draftAssignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id,
            deadline: _clock.UtcNow.AddDays(7),
            status: AssignmentStatus.Draft);

        var service = new AssignmentService(_db, _clock);
        var studentInClassA = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByIdAsync(draftAssignment.Id, studentInClassA));
    }

    [Fact]
    public async Task GetByIdAsync_Throws_NotFound_WhenAssignmentIsInAnotherClass()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var classBAssignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassBSubject.Id, seed.Teacher.Id,
            deadline: _clock.UtcNow.AddDays(7),
            status: AssignmentStatus.Published);

        var service = new AssignmentService(_db, _clock);
        var studentInClassA = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByIdAsync(classBAssignment.Id, studentInClassA));
    }

    [Fact]
    public async Task GetByIdAsync_Succeeds_WhenAssignmentIsPublishedInStudentsOwnClass()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var publishedAssignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id,
            deadline: _clock.UtcNow.AddDays(7),
            status: AssignmentStatus.Published);

        var service = new AssignmentService(_db, _clock);
        var studentInClassA = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);

        var result = await service.GetByIdAsync(publishedAssignment.Id, studentInClassA);

        Assert.Equal(publishedAssignment.Id, result.Id);
    }
}
