namespace AssignmentSystem.Api.DTOs.Classes;

public record ClassCourseResponse(int Id, string Name, string? Description, bool IsActive);

public record CreateClassCourseRequest(string Name, string? Description);

public record UpdateClassCourseRequest(string Name, string? Description, bool IsActive);
