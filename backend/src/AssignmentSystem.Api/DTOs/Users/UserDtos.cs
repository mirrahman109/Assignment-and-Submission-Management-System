namespace AssignmentSystem.Api.DTOs.Users;

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    string Role,
    int? ClassCourseId,
    string? ClassCourseName,
    bool IsActive,
    DateTime CreatedAt);

public record CreateUserRequest(string FullName, string Email, string Password, string Role, int? ClassCourseId);

public record UpdateUserRequest(string FullName, int? ClassCourseId);
