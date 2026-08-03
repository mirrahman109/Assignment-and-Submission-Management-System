namespace AssignmentSystem.Api.DTOs.TeacherAssignments;

public record TeacherAssignmentResponse(
    int Id,
    int TeacherId,
    string TeacherName,
    int ClassSubjectId,
    string ClassCourseName,
    string SubjectName);

public record CreateTeacherAssignmentRequest(int TeacherId, int ClassSubjectId);
