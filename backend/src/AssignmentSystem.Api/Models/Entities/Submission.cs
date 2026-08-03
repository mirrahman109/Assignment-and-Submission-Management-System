using AssignmentSystem.Api.Models.Entities.Enums;

namespace AssignmentSystem.Api.Models.Entities;

public class Submission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public string AnswerText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsLate { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public decimal? Marks { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
    public int? GradedByTeacherId { get; set; }
    public User? GradedByTeacher { get; set; }
}
