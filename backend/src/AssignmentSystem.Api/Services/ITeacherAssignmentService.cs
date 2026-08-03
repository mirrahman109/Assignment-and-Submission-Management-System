using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.TeacherAssignments;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface ITeacherAssignmentService
{
    Task<List<TeacherAssignmentResponse>> ListAsync(ICurrentUserService currentUser);
    Task<TeacherAssignmentResponse> CreateAsync(CreateTeacherAssignmentRequest request);
    Task DeleteAsync(int id);
}

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public TeacherAssignmentService(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<List<TeacherAssignmentResponse>> ListAsync(ICurrentUserService currentUser)
    {
        IQueryable<TeacherSubjectAssignment> query = _db.TeacherSubjectAssignments
            .Include(t => t.Teacher)
            .Include(t => t.ClassSubject).ThenInclude(cs => cs.ClassCourse)
            .Include(t => t.ClassSubject).ThenInclude(cs => cs.Subject);

        if (currentUser.Role == RoleType.Teacher)
        {
            query = query.Where(t => t.TeacherId == currentUser.UserId);
        }

        var items = await query.OrderBy(t => t.Teacher.FullName).ToListAsync();
        return items.Select(ToResponse).ToList();
    }

    public async Task<TeacherAssignmentResponse> CreateAsync(CreateTeacherAssignmentRequest request)
    {
        var teacher = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.TeacherId && u.Role == RoleType.Teacher)
            ?? throw new NotFoundException("Teacher not found.");

        var classSubject = await _db.ClassSubjects
            .Include(cs => cs.ClassCourse).Include(cs => cs.Subject)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassSubjectId)
            ?? throw new NotFoundException("Class-subject link not found.");

        var duplicate = await _db.TeacherSubjectAssignments.AnyAsync(t =>
            t.TeacherId == request.TeacherId && t.ClassSubjectId == request.ClassSubjectId);
        if (duplicate) throw new ConflictException("This teacher is already assigned to this class/subject.");

        var entity = new TeacherSubjectAssignment
        {
            TeacherId = request.TeacherId,
            ClassSubjectId = request.ClassSubjectId,
            CreatedAt = _clock.UtcNow
        };
        _db.TeacherSubjectAssignments.Add(entity);
        await _db.SaveChangesAsync();

        return new TeacherAssignmentResponse(
            entity.Id, teacher.Id, teacher.FullName, classSubject.Id, classSubject.ClassCourse.Name, classSubject.Subject.Name);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.TeacherSubjectAssignments.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException("Teacher assignment not found.");
        _db.TeacherSubjectAssignments.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private static TeacherAssignmentResponse ToResponse(TeacherSubjectAssignment t) => new(
        t.Id, t.TeacherId, t.Teacher.FullName, t.ClassSubjectId, t.ClassSubject.ClassCourse.Name, t.ClassSubject.Subject.Name);
}
