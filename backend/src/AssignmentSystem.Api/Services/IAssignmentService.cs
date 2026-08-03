using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Assignments;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface IAssignmentService
{
    Task<List<AssignmentResponse>> ListAsync(ICurrentUserService currentUser);
    Task<AssignmentResponse> GetByIdAsync(int id, ICurrentUserService currentUser);
    Task<AssignmentResponse> CreateAsync(CreateAssignmentRequest request, ICurrentUserService currentUser);
    Task<AssignmentResponse> UpdateAsync(int id, UpdateAssignmentRequest request, ICurrentUserService currentUser);
    Task<AssignmentResponse> UpdateStatusAsync(int id, string status, ICurrentUserService currentUser);
    Task DeleteAsync(int id, ICurrentUserService currentUser);
}

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public AssignmentService(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<List<AssignmentResponse>> ListAsync(ICurrentUserService currentUser)
    {
        var query = BaseQuery();

        switch (currentUser.Role)
        {
            case RoleType.Teacher:
                query = query.Where(a => a.TeacherId == currentUser.UserId);
                break;
            case RoleType.Student:
                var studentClassId = await GetStudentClassIdAsync(currentUser.UserId);
                query = query.Where(a =>
                    a.Status == AssignmentStatus.Published &&
                    a.ClassSubject.ClassCourseId == studentClassId);
                break;
            case RoleType.Admin:
            default:
                break;
        }

        var assignments = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return assignments.Select(ToResponse).ToList();
    }

    public async Task<AssignmentResponse> GetByIdAsync(int id, ICurrentUserService currentUser)
    {
        var assignment = await BaseQuery().FirstOrDefaultAsync(a => a.Id == id);
        if (assignment is null)
        {
            throw new NotFoundException("Assignment not found.");
        }

        if (currentUser.Role == RoleType.Student)
        {
            var studentClassId = await GetStudentClassIdAsync(currentUser.UserId);
            var visible = assignment.Status == AssignmentStatus.Published &&
                          assignment.ClassSubject.ClassCourseId == studentClassId;
            // 404 (not 403) so a student can't distinguish "draft" from "doesn't exist" or "other class".
            if (!visible) throw new NotFoundException("Assignment not found.");
        }
        else if (currentUser.Role == RoleType.Teacher && assignment.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own this assignment.");
        }

        return ToResponse(assignment);
    }

    public async Task<AssignmentResponse> CreateAsync(CreateAssignmentRequest request, ICurrentUserService currentUser)
    {
        if (request.Deadline <= _clock.UtcNow)
        {
            throw new ValidationAppException("deadline", "Deadline must be in the future.");
        }
        if (request.MaxMarks <= 0)
        {
            throw new ValidationAppException("maxMarks", "Max marks must be greater than zero.");
        }

        var isAssigned = await _db.TeacherSubjectAssignments.AnyAsync(t =>
            t.TeacherId == currentUser.UserId && t.ClassSubjectId == request.ClassSubjectId);
        if (!isAssigned)
        {
            throw new ForbiddenException("You are not assigned to this class/subject.");
        }

        var entity = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            ClassSubjectId = request.ClassSubjectId,
            TeacherId = currentUser.UserId,
            Deadline = request.Deadline,
            MaxMarks = request.MaxMarks,
            AllowLateSubmission = request.AllowLateSubmission,
            Status = request.PublishImmediately ? AssignmentStatus.Published : AssignmentStatus.Draft,
            CreatedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow
        };

        _db.Assignments.Add(entity);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(entity.Id, currentUser);
    }

    public async Task<AssignmentResponse> UpdateAsync(int id, UpdateAssignmentRequest request, ICurrentUserService currentUser)
    {
        var entity = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Assignment not found.");

        if (entity.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own this assignment.");
        }
        if (request.MaxMarks <= 0)
        {
            throw new ValidationAppException("maxMarks", "Max marks must be greater than zero.");
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Deadline = request.Deadline;
        entity.MaxMarks = request.MaxMarks;
        entity.AllowLateSubmission = request.AllowLateSubmission;
        entity.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(entity.Id, currentUser);
    }

    public async Task<AssignmentResponse> UpdateStatusAsync(int id, string status, ICurrentUserService currentUser)
    {
        var entity = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Assignment not found.");

        if (entity.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own this assignment.");
        }
        if (!Enum.TryParse<AssignmentStatus>(status, out var parsed))
        {
            throw new ValidationAppException("status", "Status must be Draft or Published.");
        }

        entity.Status = parsed;
        entity.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(entity.Id, currentUser);
    }

    public async Task DeleteAsync(int id, ICurrentUserService currentUser)
    {
        var entity = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Assignment not found.");

        if (entity.TeacherId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not own this assignment.");
        }

        var hasSubmissions = await _db.Submissions.AnyAsync(s => s.AssignmentId == id);
        if (hasSubmissions)
        {
            throw new ConflictException("Cannot delete an assignment that already has submissions.");
        }

        _db.Assignments.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private async Task<int?> GetStudentClassIdAsync(int studentId)
    {
        return await _db.Users.Where(u => u.Id == studentId).Select(u => u.ClassCourseId).FirstOrDefaultAsync();
    }

    private IQueryable<Assignment> BaseQuery() => _db.Assignments
        .Include(a => a.ClassSubject).ThenInclude(cs => cs.ClassCourse)
        .Include(a => a.ClassSubject).ThenInclude(cs => cs.Subject)
        .Include(a => a.Teacher);

    private static AssignmentResponse ToResponse(Assignment a) => new(
        a.Id, a.Title, a.Description, a.ClassSubjectId, a.ClassSubject.ClassCourse.Name, a.ClassSubject.Subject.Name,
        a.TeacherId, a.Teacher.FullName, a.Deadline, a.MaxMarks, a.AllowLateSubmission, a.Status.ToString(), a.CreatedAt);
}
