namespace AssignmentSystem.Api.Models.Entities;

/// <summary>
/// The row every teacher-side authorization check hinges on: proves a given teacher
/// is allowed to manage a given (class, subject) combination.
/// </summary>
public class TeacherSubjectAssignment
{
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public int ClassSubjectId { get; set; }
    public ClassSubject ClassSubject { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
