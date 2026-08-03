using AssignmentSystem.Api.Models.Entities.Enums;

namespace AssignmentSystem.Api.Models.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RoleType Role { get; set; }

    /// <summary>Only meaningful when Role == Student.</summary>
    public int? ClassCourseId { get; set; }
    public ClassCourse? ClassCourse { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
