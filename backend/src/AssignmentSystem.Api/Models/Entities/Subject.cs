namespace AssignmentSystem.Api.Models.Entities;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
}
