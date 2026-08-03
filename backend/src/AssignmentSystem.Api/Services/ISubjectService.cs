using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Subjects;
using AssignmentSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface ISubjectService
{
    Task<List<SubjectResponse>> ListAsync();
    Task<SubjectResponse> CreateAsync(CreateSubjectRequest request);
    Task<SubjectResponse> UpdateAsync(int id, UpdateSubjectRequest request);
    Task DeleteAsync(int id);
}

public class SubjectService : ISubjectService
{
    private readonly AppDbContext _db;

    public SubjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SubjectResponse>> ListAsync()
    {
        var subjects = await _db.Subjects.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        return subjects.Select(ToResponse).ToList();
    }

    public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request)
    {
        var entity = new Subject { Name = request.Name, Code = request.Code, IsActive = true };
        _db.Subjects.Add(entity);
        await _db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task<SubjectResponse> UpdateAsync(int id, UpdateSubjectRequest request)
    {
        var entity = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Subject not found.");

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        return ToResponse(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Subject not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync();
    }

    private static SubjectResponse ToResponse(Subject s) => new(s.Id, s.Name, s.Code, s.IsActive);
}
