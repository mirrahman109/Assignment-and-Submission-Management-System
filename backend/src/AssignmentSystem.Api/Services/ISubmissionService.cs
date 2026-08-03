using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Submissions;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface ISubmissionService
{
    Task<SubmissionResponse> CreateAsync(int assignmentId, CreateSubmissionRequest request, ICurrentUserService currentUser);
    Task<List<SubmissionResponse>> ListForAssignmentAsync(int assignmentId, ICurrentUserService currentUser);
    Task<List<SubmissionResponse>> ListMineAsync(ICurrentUserService currentUser);
    Task<SubmissionResponse> GetByIdAsync(int id, ICurrentUserService currentUser);
    Task<SubmissionResponse> UpdateAsync(int id, UpdateSubmissionRequest request, ICurrentUserService currentUser);
    Task<SubmissionResponse> GradeAsync(int id, GradeSubmissionRequest request, ICurrentUserService currentUser);
    Task<SubmissionResponse> UpdateStatusAsync(int id, string status, ICurrentUserService currentUser);
}

public class SubmissionService : ISubmissionService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public SubmissionService(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<SubmissionResponse> CreateAsync(int assignmentId, CreateSubmissionRequest request, ICurrentUserService currentUser)
    {
        var assignment = await _db.Assignments.Include(a => a.ClassSubject)
            .FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        var studentClassId = await _db.Users.Where(u => u.Id == currentUser.UserId)
            .Select(u => u.ClassCourseId).FirstOrDefaultAsync();

        if (assignment.Status != AssignmentStatus.Published || assignment.ClassSubject.ClassCourseId != studentClassId)
        {
            throw new ForbiddenException("This assignment is not available to you.");
        }

        var now = _clock.UtcNow;
        var isLate = now > assignment.Deadline;
        if (isLate && !assignment.AllowLateSubmission)
        {
            throw new ValidationAppException("deadline", "The deadline for this assignment has passed.");
        }

        var alreadyExists = await _db.Submissions.AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == currentUser.UserId);
        if (alreadyExists)
        {
            throw new ConflictException("You have already submitted this assignment. Use update instead.");
        }

        var entity = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = currentUser.UserId,
            AnswerText = request.AnswerText,
            AttachmentUrl = request.AttachmentUrl,
            SubmittedAt = now,
            IsLate = isLate,
            Status = SubmissionStatus.Submitted
        };

        _db.Submissions.Add(entity);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(entity.Id, currentUser);
    }

    public async Task<List<SubmissionResponse>> ListForAssignmentAsync(int assignmentId, ICurrentUserService currentUser)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new NotFoundException("Assignment not found.");

        if (currentUser.Role == RoleType.Teacher && assignment.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own this assignment.");
        }

        var submissions = await BaseQuery().Where(s => s.AssignmentId == assignmentId)
            .OrderBy(s => s.Student.FullName).ToListAsync();
        return submissions.Select(ToResponse).ToList();
    }

    public async Task<List<SubmissionResponse>> ListMineAsync(ICurrentUserService currentUser)
    {
        var submissions = await BaseQuery().Where(s => s.StudentId == currentUser.UserId)
            .OrderByDescending(s => s.SubmittedAt).ToListAsync();
        return submissions.Select(ToResponse).ToList();
    }

    public async Task<SubmissionResponse> GetByIdAsync(int id, ICurrentUserService currentUser)
    {
        var submission = await BaseQuery().FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Submission not found.");

        EnsureCanView(submission, currentUser);
        return ToResponse(submission);
    }

    public async Task<SubmissionResponse> UpdateAsync(int id, UpdateSubmissionRequest request, ICurrentUserService currentUser)
    {
        var submission = await _db.Submissions.Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Submission not found.");

        if (submission.StudentId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own this submission.");
        }

        if (submission.Status == SubmissionStatus.Graded)
        {
            throw new ForbiddenException("This submission has already been graded and can no longer be edited.");
        }

        var now = _clock.UtcNow;
        var isLate = now > submission.Assignment.Deadline;
        if (isLate && !submission.Assignment.AllowLateSubmission)
        {
            throw new ForbiddenException("The deadline for this assignment has passed and late submissions are not allowed.");
        }

        submission.AnswerText = request.AnswerText;
        submission.AttachmentUrl = request.AttachmentUrl;
        submission.UpdatedAt = now;
        submission.IsLate = isLate;
        if (submission.Status == SubmissionStatus.NeedsRevision)
        {
            submission.Status = SubmissionStatus.Submitted;
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(submission.Id, currentUser);
    }

    public async Task<SubmissionResponse> GradeAsync(int id, GradeSubmissionRequest request, ICurrentUserService currentUser)
    {
        var submission = await _db.Submissions.Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Submission not found.");

        if (currentUser.Role == RoleType.Teacher && submission.Assignment.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own the assignment for this submission.");
        }

        if (request.Marks < 0)
        {
            throw new ValidationAppException("marks", "Marks cannot be negative.");
        }
        if (request.Marks > submission.Assignment.MaxMarks)
        {
            throw new ValidationAppException("marks", $"Marks cannot exceed the maximum of {submission.Assignment.MaxMarks}.");
        }

        submission.Marks = request.Marks;
        submission.Feedback = request.Feedback;
        submission.Status = SubmissionStatus.Graded;
        submission.GradedAt = _clock.UtcNow;
        submission.GradedByTeacherId = currentUser.UserId;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(submission.Id, currentUser);
    }

    public async Task<SubmissionResponse> UpdateStatusAsync(int id, string status, ICurrentUserService currentUser)
    {
        var submission = await _db.Submissions.Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Submission not found.");

        if (currentUser.Role == RoleType.Teacher && submission.Assignment.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own the assignment for this submission.");
        }

        if (!Enum.TryParse<SubmissionStatus>(status, out var parsed))
        {
            throw new ValidationAppException("status", "Status must be Submitted, NeedsRevision, or Graded.");
        }

        submission.Status = parsed;
        if (parsed == SubmissionStatus.NeedsRevision)
        {
            submission.Marks = null;
            submission.GradedAt = null;
            submission.GradedByTeacherId = null;
        }

        await _db.SaveChangesAsync();
        return await GetByIdAsync(submission.Id, currentUser);
    }

    private void EnsureCanView(Submission submission, ICurrentUserService currentUser)
    {
        switch (currentUser.Role)
        {
            case RoleType.Admin:
                return;
            case RoleType.Student when submission.StudentId == currentUser.UserId:
                return;
            case RoleType.Teacher when submission.Assignment.TeacherId == currentUser.UserId:
                return;
            default:
                throw new ForbiddenException("You do not have access to this submission.");
        }
    }

    private IQueryable<Submission> BaseQuery() => _db.Submissions
        .Include(s => s.Assignment)
        .Include(s => s.Student);

    private static SubmissionResponse ToResponse(Submission s) => new(
        s.Id, s.AssignmentId, s.Assignment.Title, s.StudentId, s.Student.FullName, s.AnswerText, s.AttachmentUrl,
        s.SubmittedAt, s.UpdatedAt, s.IsLate, s.Status.ToString(), s.Marks, s.Assignment.MaxMarks, s.Feedback, s.GradedAt);
}
