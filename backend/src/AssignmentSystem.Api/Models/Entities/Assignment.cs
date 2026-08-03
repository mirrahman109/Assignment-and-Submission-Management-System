using AssignmentSystem.Api.Models.Entities.Enums;

namespace AssignmentSystem.Api.Models.Entities;

public class Assignment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int ClassSubjectId { get; set; }
    public ClassSubject ClassSubject { get; set; } = null!;

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public DateTime Deadline { get; set; }
    public decimal MaxMarks { get; set; }
    public bool AllowLateSubmission { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
