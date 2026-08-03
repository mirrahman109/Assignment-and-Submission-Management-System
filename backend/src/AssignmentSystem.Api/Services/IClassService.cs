using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Classes;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface IClassService
{
    Task<List<ClassCourseResponse>> ListAsync(ICurrentUserService currentUser);
    Task<ClassCourseResponse> CreateAsync(CreateClassCourseRequest request);
    Task<ClassCourseResponse> UpdateAsync(int id, UpdateClassCourseRequest request);
    Task DeleteAsync(int id);
}

public class ClassService : IClassService
{
    private readonly AppDbContext _db;

    public ClassService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClassCourseResponse>> ListAsync(ICurrentUserService currentUser)
    {
        IQueryable<ClassCourse> query = _db.ClassCourses;

        if (currentUser.Role == RoleType.Student)
        {
            query = query.Where(c => c.Students.Any(s => s.Id == currentUser.UserId));
        }
        else if (currentUser.Role == RoleType.Teacher)
        {
            query = query.Where(c => c.ClassSubjects.Any(cs =>
                cs.TeacherAssignments.Any(ta => ta.TeacherId == currentUser.UserId)));
        }

        var classes = await query.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        return classes.Select(ToResponse).ToList();
    }

    public async Task<ClassCourseResponse> CreateAsync(CreateClassCourseRequest request)
    {
        var entity = new ClassCourse { Name = request.Name, Description = request.Description, IsActive = true };
        _db.ClassCourses.Add(entity);
        await _db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task<ClassCourseResponse> UpdateAsync(int id, UpdateClassCourseRequest request)
    {
        var entity = await _db.ClassCourses.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Class/course not found.");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.ClassCourses.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Class/course not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync();
    }

    private static ClassCourseResponse ToResponse(ClassCourse c) => new(c.Id, c.Name, c.Description, c.IsActive);
}
