using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;

namespace AssignmentSystem.UnitTests.Services;

public class SubmissionServiceLeakTests : IDisposable
{
    private readonly SqliteInMemoryDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly FakeClock _clock;

    public SubmissionServiceLeakTests()
    {
        _factory = new SqliteInMemoryDbContextFactory();
        _db = _factory.Context;
        _clock = new FakeClock();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetByIdAsync_Throws_WhenStudentIsNotTheOwner()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id, deadline: _clock.UtcNow.AddDays(7));

        var service = new SubmissionService(_db, _clock);
        var owner = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);
        var submission = await service.CreateAsync(assignment.Id, new CreateSubmissionRequest("my answer", null), owner);

        var otherStudent = new FakeCurrentUserService(seed.StudentB.Id, RoleType.Student);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetByIdAsync(submission.Id, otherStudent));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenStudentIsNotTheOwner()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id, deadline: _clock.UtcNow.AddDays(7));

        var service = new SubmissionService(_db, _clock);
        var owner = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);
        var submission = await service.CreateAsync(assignment.Id, new CreateSubmissionRequest("my answer", null), owner);

        var otherStudent = new FakeCurrentUserService(seed.StudentB.Id, RoleType.Student);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateAsync(submission.Id, new UpdateSubmissionRequest("hijacked answer", null), otherStudent));
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenTeacherDoesNotOwnTheAssignment()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id, deadline: _clock.UtcNow.AddDays(7));

        var service = new SubmissionService(_db, _clock);
        var owner = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);
        var submission = await service.CreateAsync(assignment.Id, new CreateSubmissionRequest("my answer", null), owner);

        // seed.OtherTeacher is not assigned to seed.ClassASubject, so does not own this assignment.
        var unrelatedTeacher = new FakeCurrentUserService(seed.OtherTeacher.Id, RoleType.Teacher);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetByIdAsync(submission.Id, unrelatedTeacher));
    }

    [Fact]
    public async Task GradeAsync_Throws_WhenTeacherDoesNotOwnTheAssignment()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id, deadline: _clock.UtcNow.AddDays(7));

        var service = new SubmissionService(_db, _clock);
        var owner = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);
        var submission = await service.CreateAsync(assignment.Id, new CreateSubmissionRequest("my answer", null), owner);

        var unrelatedTeacher = new FakeCurrentUserService(seed.OtherTeacher.Id, RoleType.Teacher);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GradeAsync(submission.Id, new GradeSubmissionRequest(50, "nice try"), unrelatedTeacher));
    }
}
