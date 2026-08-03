namespace AssignmentSystem.Api.DTOs.Assignments;

public record AssignmentResponse(
    int Id,
    string Title,
    string Description,
    int ClassSubjectId,
    string ClassCourseName,
    string SubjectName,
    int TeacherId,
    string TeacherName,
    DateTime Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission,
    string Status,
    DateTime CreatedAt);

public record CreateAssignmentRequest(
    string Title,
    string Description,
    int ClassSubjectId,
    DateTime Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission,
    bool PublishImmediately);

public record UpdateAssignmentRequest(
    string Title,
    string Description,
    DateTime Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission);

public record UpdateAssignmentStatusRequest(string Status);
