using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;

namespace AssignmentSystem.UnitTests.Fixtures;

/// <summary>Seeds a minimal, known graph shared across service tests: two classes, one subject,
/// one class-subject link per class, a teacher assigned only to class A, and one student per class.</summary>
public static class TestDataBuilder
{
    public static (ClassCourse ClassA, ClassCourse ClassB, Subject Math, ClassSubject ClassASubject,
        ClassSubject ClassBSubject, User Teacher, User OtherTeacher, User StudentA, User StudentB) SeedBasicGraph(AppDbContext db)
    {
        var classA = new ClassCourse { Name = "Class A", IsActive = true };
        var classB = new ClassCourse { Name = "Class B", IsActive = true };
        var math = new Subject { Name = "Mathematics", Code = "MATH101", IsActive = true };
        db.ClassCourses.AddRange(classA, classB);
        db.Subjects.Add(math);
        db.SaveChanges();

        var classASubject = new ClassSubject { ClassCourseId = classA.Id, SubjectId = math.Id };
        var classBSubject = new ClassSubject { ClassCourseId = classB.Id, SubjectId = math.Id };
        db.ClassSubjects.AddRange(classASubject, classBSubject);
        db.SaveChanges();

        var teacher = new User { FullName = "Teacher One", Email = "teacher1@test.local", PasswordHash = "x", Role = RoleType.Teacher, IsActive = true };
        var otherTeacher = new User { FullName = "Teacher Two", Email = "teacher2@test.local", PasswordHash = "x", Role = RoleType.Teacher, IsActive = true };
        var studentA = new User { FullName = "Student A", Email = "studentA@test.local", PasswordHash = "x", Role = RoleType.Student, ClassCourseId = classA.Id, IsActive = true };
        var studentB = new User { FullName = "Student B", Email = "studentB@test.local", PasswordHash = "x", Role = RoleType.Student, ClassCourseId = classB.Id, IsActive = true };
        db.Users.AddRange(teacher, otherTeacher, studentA, studentB);
        db.SaveChanges();

        db.TeacherSubjectAssignments.Add(new TeacherSubjectAssignment
        {
            TeacherId = teacher.Id,
            ClassSubjectId = classASubject.Id,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        return (classA, classB, math, classASubject, classBSubject, teacher, otherTeacher, studentA, studentB);
    }

    public static Assignment CreateAssignment(
        AppDbContext db, int classSubjectId, int teacherId, DateTime deadline,
        decimal maxMarks = 100, bool allowLate = false, AssignmentStatus status = AssignmentStatus.Published)
    {
        var assignment = new Assignment
        {
            Title = "Test Assignment",
            Description = "Test description",
            ClassSubjectId = classSubjectId,
            TeacherId = teacherId,
            Deadline = deadline,
            MaxMarks = maxMarks,
            AllowLateSubmission = allowLate,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Assignments.Add(assignment);
        db.SaveChanges();
        return assignment;
    }
}
