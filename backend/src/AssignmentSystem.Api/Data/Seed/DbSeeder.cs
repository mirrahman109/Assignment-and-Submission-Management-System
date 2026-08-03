using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Data.Seed;

/// <summary>Idempotent — safe to run on every startup; only seeds when the Users table is empty.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var classA = new ClassCourse { Name = "Grade 10 - A", Description = "Section A", IsActive = true };
        var classB = new ClassCourse { Name = "Grade 10 - B", Description = "Section B", IsActive = true };
        db.ClassCourses.AddRange(classA, classB);

        var math = new Subject { Name = "Mathematics", Code = "MATH101", IsActive = true };
        var english = new Subject { Name = "English", Code = "ENG101", IsActive = true };
        var science = new Subject { Name = "Science", Code = "SCI101", IsActive = true };
        db.Subjects.AddRange(math, english, science);
        await db.SaveChangesAsync();

        var admin = CreateUser(passwordHasher, "System Admin", "admin@school.test", "Admin@123", RoleType.Admin, null, now);
        var teacherMath = CreateUser(passwordHasher, "Rahim Uddin", "teacher.math@school.test", "Teacher@123", RoleType.Teacher, null, now);
        var teacherEnglish = CreateUser(passwordHasher, "Karim Ahmed", "teacher.english@school.test", "Teacher@123", RoleType.Teacher, null, now);
        db.Users.AddRange(admin, teacherMath, teacherEnglish);
        await db.SaveChangesAsync();

        var studentA1 = CreateUser(passwordHasher, "Ayesha Khan", "student1@school.test", "Student@123", RoleType.Student, classA.Id, now);
        var studentA2 = CreateUser(passwordHasher, "Bilal Hossain", "student2@school.test", "Student@123", RoleType.Student, classA.Id, now);
        var studentB1 = CreateUser(passwordHasher, "Chitra Roy", "student3@school.test", "Student@123", RoleType.Student, classB.Id, now);
        db.Users.AddRange(studentA1, studentA2, studentB1);
        await db.SaveChangesAsync();

        var classAMath = new ClassSubject { ClassCourseId = classA.Id, SubjectId = math.Id };
        var classAEnglish = new ClassSubject { ClassCourseId = classA.Id, SubjectId = english.Id };
        var classBMath = new ClassSubject { ClassCourseId = classB.Id, SubjectId = math.Id };
        var classBScience = new ClassSubject { ClassCourseId = classB.Id, SubjectId = science.Id };
        db.ClassSubjects.AddRange(classAMath, classAEnglish, classBMath, classBScience);
        await db.SaveChangesAsync();

        db.TeacherSubjectAssignments.AddRange(
            new TeacherSubjectAssignment { TeacherId = teacherMath.Id, ClassSubjectId = classAMath.Id, CreatedAt = now },
            new TeacherSubjectAssignment { TeacherId = teacherMath.Id, ClassSubjectId = classBMath.Id, CreatedAt = now },
            new TeacherSubjectAssignment { TeacherId = teacherEnglish.Id, ClassSubjectId = classAEnglish.Id, CreatedAt = now });
        await db.SaveChangesAsync();

        var publishedAssignment = new Assignment
        {
            Title = "Algebra Basics — Worksheet 1",
            Description = "Solve the attached algebra problems and show your work.",
            ClassSubjectId = classAMath.Id,
            TeacherId = teacherMath.Id,
            Deadline = now.AddDays(7),
            MaxMarks = 100,
            AllowLateSubmission = true,
            Status = AssignmentStatus.Published,
            CreatedAt = now,
            UpdatedAt = now
        };
        var draftAssignment = new Assignment
        {
            Title = "Essay: My Summer Vacation",
            Description = "Write a 300-word essay about your summer vacation.",
            ClassSubjectId = classAEnglish.Id,
            TeacherId = teacherEnglish.Id,
            Deadline = now.AddDays(10),
            MaxMarks = 50,
            AllowLateSubmission = false,
            Status = AssignmentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };
        var gradedAssignment = new Assignment
        {
            Title = "Algebra Basics — Worksheet 0 (past)",
            Description = "Warm-up worksheet, already graded for demo purposes.",
            ClassSubjectId = classAMath.Id,
            TeacherId = teacherMath.Id,
            Deadline = now.AddDays(-3),
            MaxMarks = 20,
            AllowLateSubmission = false,
            Status = AssignmentStatus.Published,
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now.AddDays(-10)
        };
        db.Assignments.AddRange(publishedAssignment, draftAssignment, gradedAssignment);
        await db.SaveChangesAsync();

        db.Submissions.Add(new Submission
        {
            AssignmentId = gradedAssignment.Id,
            StudentId = studentA1.Id,
            AnswerText = "Here are my answers to the warm-up worksheet.",
            SubmittedAt = now.AddDays(-8),
            IsLate = false,
            Status = SubmissionStatus.Graded,
            Marks = 18,
            Feedback = "Great work, minor mistake on question 4.",
            GradedAt = now.AddDays(-7),
            GradedByTeacherId = teacherMath.Id
        });
        await db.SaveChangesAsync();
    }

    private static User CreateUser(
        IPasswordHasher<User> passwordHasher, string fullName, string email, string password,
        RoleType role, int? classCourseId, DateTime now)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email,
            Role = role,
            ClassCourseId = classCourseId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        return user;
    }
}
