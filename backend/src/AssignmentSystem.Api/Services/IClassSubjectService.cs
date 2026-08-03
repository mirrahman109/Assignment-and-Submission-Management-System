using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.ClassSubjects;
using AssignmentSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface IClassSubjectService
{
    Task<List<ClassSubjectResponse>> ListAsync();
    Task<ClassSubjectResponse> CreateAsync(CreateClassSubjectRequest request);
    Task DeleteAsync(int id);
}

public class ClassSubjectService : IClassSubjectService
{
    private readonly AppDbContext _db;

    public ClassSubjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClassSubjectResponse>> ListAsync()
    {
        var items = await _db.ClassSubjects
            .Include(cs => cs.ClassCourse)
            .Include(cs => cs.Subject)
            .OrderBy(cs => cs.ClassCourse.Name).ThenBy(cs => cs.Subject.Name)
            .ToListAsync();
        return items.Select(ToResponse).ToList();
    }

    public async Task<ClassSubjectResponse> CreateAsync(CreateClassSubjectRequest request)
    {
        var classExists = await _db.ClassCourses.AnyAsync(c => c.Id == request.ClassCourseId);
        if (!classExists) throw new NotFoundException("Class/course not found.");

        var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == request.SubjectId);
        if (!subjectExists) throw new NotFoundException("Subject not found.");

        var duplicate = await _db.ClassSubjects.AnyAsync(cs =>
            cs.ClassCourseId == request.ClassCourseId && cs.SubjectId == request.SubjectId);
        if (duplicate) throw new ConflictException("This subject is already linked to this class/course.");

        var entity = new ClassSubject { ClassCourseId = request.ClassCourseId, SubjectId = request.SubjectId };
        _db.ClassSubjects.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(e => e.ClassCourse).LoadAsync();
        await _db.Entry(entity).Reference(e => e.Subject).LoadAsync();
        return ToResponse(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.ClassSubjects.FirstOrDefaultAsync(cs => cs.Id == id)
            ?? throw new NotFoundException("Class-subject link not found.");

        var hasAssignments = await _db.Assignments.AnyAsync(a => a.ClassSubjectId == id);
        if (hasAssignments) throw new ConflictException("Cannot remove a class-subject link that has assignments.");

        _db.ClassSubjects.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private static ClassSubjectResponse ToResponse(ClassSubject cs) => new(
        cs.Id, cs.ClassCourseId, cs.ClassCourse.Name, cs.SubjectId, cs.Subject.Name);
}
