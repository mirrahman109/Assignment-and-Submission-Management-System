namespace AssignmentSystem.Api.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiresAtUtc, UserSummary User);

public record UserSummary(int Id, string FullName, string Email, string Role, int? ClassCourseId);
