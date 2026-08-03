namespace AssignmentSystem.Api.DTOs.Subjects;

public record SubjectResponse(int Id, string Name, string Code, bool IsActive);

public record CreateSubjectRequest(string Name, string Code);

public record UpdateSubjectRequest(string Name, string Code, bool IsActive);
