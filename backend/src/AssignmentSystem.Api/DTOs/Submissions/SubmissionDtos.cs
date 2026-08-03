namespace AssignmentSystem.Api.DTOs.Submissions;

public record SubmissionResponse(
    int Id,
    int AssignmentId,
    string AssignmentTitle,
    int StudentId,
    string StudentName,
    string AnswerText,
    string? AttachmentUrl,
    DateTime SubmittedAt,
    DateTime? UpdatedAt,
    bool IsLate,
    string Status,
    decimal? Marks,
    decimal MaxMarks,
    string? Feedback,
    DateTime? GradedAt);

public record CreateSubmissionRequest(string AnswerText, string? AttachmentUrl);

public record UpdateSubmissionRequest(string AnswerText, string? AttachmentUrl);

public record GradeSubmissionRequest(decimal Marks, string? Feedback);

public record UpdateSubmissionStatusRequest(string Status);
