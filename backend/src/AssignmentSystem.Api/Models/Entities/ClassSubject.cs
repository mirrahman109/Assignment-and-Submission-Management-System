namespace AssignmentSystem.Api.Models.Entities;

/// <summary>Join entity binding a subject to a class/course (many-to-many).</summary>
public class ClassSubject
{
    public int Id { get; set; }

    public int ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public ICollection<TeacherSubjectAssignment> TeacherAssignments { get; set; } = new List<TeacherSubjectAssignment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
