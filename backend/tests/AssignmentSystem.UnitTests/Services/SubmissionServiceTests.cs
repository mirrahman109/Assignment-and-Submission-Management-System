using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;

namespace AssignmentSystem.UnitTests.Services;

public class SubmissionServiceTests : IDisposable
{
    private readonly SqliteInMemoryDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly FakeClock _clock;

    public SubmissionServiceTests()
    {
        _factory = new SqliteInMemoryDbContextFactory();
        _db = _factory.Context;
        _clock = new FakeClock();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task CreateAsync_Throws_Conflict_OnDuplicateSubmission()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id, deadline: _clock.UtcNow.AddDays(7));

        var service = new SubmissionService(_db, _clock);
        var currentUser = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);

        await service.CreateAsync(assignment.Id, new CreateSubmissionRequest("first attempt", null), currentUser);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(assignment.Id, new CreateSubmissionRequest("second attempt", null), currentUser));
    }

    [Fact]
    public async Task UpdateAsync_AllowsEdit_WhenNeedsRevision_EvenPastDeadlineWithLateDisallowed()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id,
            deadline: _clock.UtcNow.AddDays(-1), allowLate: false);

        var submission = new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = seed.StudentA.Id,
            AnswerText = "first attempt",
            SubmittedAt = _clock.UtcNow.AddDays(-2),
            Status = SubmissionStatus.NeedsRevision
        };
        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();

        var service = new SubmissionService(_db, _clock);
        var currentUser = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);

        var result = await service.UpdateAsync(
            submission.Id, new UpdateSubmissionRequest("revised answer", null), currentUser);

        Assert.Equal("revised answer", result.AnswerText);
        Assert.Equal("Submitted", result.Status);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenPastDeadlineAndNotReopenedForRevision()
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id,
            deadline: _clock.UtcNow.AddDays(-1), allowLate: false);

        var submission = new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = seed.StudentA.Id,
            AnswerText = "first attempt",
            SubmittedAt = _clock.UtcNow.AddDays(-2),
            Status = SubmissionStatus.Submitted
        };
        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();

        var service = new SubmissionService(_db, _clock);
        var currentUser = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateAsync(submission.Id, new UpdateSubmissionRequest("revised answer", null), currentUser));
    }
}
