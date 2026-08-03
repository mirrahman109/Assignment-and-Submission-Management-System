using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Users;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> ListAsync(RoleType? roleFilter);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
    Task DeactivateAsync(int id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IClock _clock;

    public UserService(AppDbContext db, IPasswordHasher<User> passwordHasher, IClock clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<List<UserResponse>> ListAsync(RoleType? roleFilter)
    {
        var query = _db.Users.Include(u => u.ClassCourse).AsQueryable();
        if (roleFilter.HasValue)
        {
            query = query.Where(u => u.Role == roleFilter.Value);
        }

        var users = await query.OrderBy(u => u.FullName).ToListAsync();
        return users.Select(ToResponse).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _db.Users.Include(u => u.ClassCourse).FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException("User not found.");
        return ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        if (!Enum.TryParse<RoleType>(request.Role, out var role))
        {
            throw new ValidationAppException("role", "Role must be one of Admin, Teacher, Student.");
        }

        if (role == RoleType.Student && request.ClassCourseId is null)
        {
            throw new ValidationAppException("classCourseId", "A student must be assigned to a class/course.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = role,
            ClassCourseId = role == RoleType.Student ? request.ClassCourseId : null,
            IsActive = true,
            CreatedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(user.Id);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException("User not found.");

        user.FullName = request.FullName;
        if (user.Role == RoleType.Student)
        {
            user.ClassCourseId = request.ClassCourseId;
        }
        user.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(user.Id);
    }

    public async Task DeactivateAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException("User not found.");

        user.IsActive = false;
        user.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static UserResponse ToResponse(User u) => new(
        u.Id, u.FullName, u.Email, u.Role.ToString(), u.ClassCourseId, u.ClassCourse?.Name, u.IsActive, u.CreatedAt);
}
