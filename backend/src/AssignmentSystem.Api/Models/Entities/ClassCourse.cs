namespace AssignmentSystem.Api.Models.Entities;

public class ClassCourse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
    public ICollection<User> Students { get; set; } = new List<User>();
}
