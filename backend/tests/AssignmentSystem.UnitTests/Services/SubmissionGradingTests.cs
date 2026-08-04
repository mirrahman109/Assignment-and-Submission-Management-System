using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;

namespace AssignmentSystem.UnitTests.Services;

public class SubmissionGradingTests : IDisposable
{
    private readonly SqliteInMemoryDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly FakeClock _clock;

    public SubmissionGradingTests()
    {
        _factory = new SqliteInMemoryDbContextFactory();
        _db = _factory.Context;
        _clock = new FakeClock();
    }

    public void Dispose() => _factory.Dispose();

    private async Task<(int SubmissionId, decimal MaxMarks, int AdminId)> SeedSubmissionAsync(decimal maxMarks)
    {
        var seed = TestDataBuilder.SeedBasicGraph(_db);
        var assignment = TestDataBuilder.CreateAssignment(
            _db, seed.ClassASubject.Id, seed.Teacher.Id, deadline: _clock.UtcNow.AddDays(7), maxMarks: maxMarks);

        var service = new SubmissionService(_db, _clock);
        var owner = new FakeCurrentUserService(seed.StudentA.Id, RoleType.Student);
        var submission = await service.CreateAsync(assignment.Id, new CreateSubmissionRequest("my answer", null), owner);

        // GradeAsync stamps GradedByTeacherId = currentUser.UserId, which is FK-constrained to a real
        // Users row, so the "admin" grader needs a genuinely seeded user ID (role on the fake is what
        // the authorization check reads, not the underlying row, so reusing the teacher's ID is fine).
        return (submission.Id, maxMarks, seed.Teacher.Id);
    }

    [Fact]
    public async Task GradeAsync_Throws_WhenMarksAreNegative()
    {
        var (submissionId, _, adminId) = await SeedSubmissionAsync(maxMarks: 100);
        var service = new SubmissionService(_db, _clock);
        var admin = new FakeCurrentUserService(adminId, RoleType.Admin);

        await Assert.ThrowsAsync<ValidationAppException>(() =>
            service.GradeAsync(submissionId, new GradeSubmissionRequest(-5, "bad"), admin));
    }

    [Fact]
    public async Task GradeAsync_Throws_WhenMarksExceedMaxMarks()
    {
        var (submissionId, maxMarks, adminId) = await SeedSubmissionAsync(maxMarks: 50);
        var service = new SubmissionService(_db, _clock);
        var admin = new FakeCurrentUserService(adminId, RoleType.Admin);

        await Assert.ThrowsAsync<ValidationAppException>(() =>
            service.GradeAsync(submissionId, new GradeSubmissionRequest(maxMarks + 1, "too generous"), admin));
    }

    [Fact]
    public async Task GradeAsync_Succeeds_SetsStatusGraded_WhenMarksAreValid()
    {
        var (submissionId, maxMarks, adminId) = await SeedSubmissionAsync(maxMarks: 100);
        var service = new SubmissionService(_db, _clock);
        var admin = new FakeCurrentUserService(adminId, RoleType.Admin);

        var result = await service.GradeAsync(submissionId, new GradeSubmissionRequest(maxMarks - 10, "well done"), admin);

        Assert.Equal("Graded", result.Status);
        Assert.Equal(maxMarks - 10, result.Marks);
    }
}
