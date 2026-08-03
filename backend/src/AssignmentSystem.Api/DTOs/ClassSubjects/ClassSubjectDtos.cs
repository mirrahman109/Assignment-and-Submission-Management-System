namespace AssignmentSystem.Api.DTOs.ClassSubjects;

public record ClassSubjectResponse(
    int Id,
    int ClassCourseId,
    string ClassCourseName,
    int SubjectId,
    string SubjectName);

public record CreateClassSubjectRequest(int ClassCourseId, int SubjectId);
