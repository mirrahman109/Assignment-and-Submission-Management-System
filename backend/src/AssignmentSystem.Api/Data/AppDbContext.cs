using AssignmentSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClassCourse> ClassCourses => Set<ClassCourse>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSubject> ClassSubjects => Set<ClassSubject>();
    public DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments => Set<TeacherSubjectAssignment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(u => u.ClassCourse)
                .WithMany(c => c.Students)
                .HasForeignKey(u => u.ClassCourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClassCourse>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(150).IsRequired();
            entity.Property(s => s.Code).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<ClassSubject>(entity =>
        {
            entity.HasIndex(cs => new { cs.ClassCourseId, cs.SubjectId }).IsUnique();

            entity.HasOne(cs => cs.ClassCourse)
                .WithMany(c => c.ClassSubjects)
                .HasForeignKey(cs => cs.ClassCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cs => cs.Subject)
                .WithMany(s => s.ClassSubjects)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeacherSubjectAssignment>(entity =>
        {
            entity.HasIndex(t => new { t.TeacherId, t.ClassSubjectId }).IsUnique();

            entity.HasOne(t => t.Teacher)
                .WithMany()
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ClassSubject)
                .WithMany(cs => cs.TeacherAssignments)
                .HasForeignKey(t => t.ClassSubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Description).IsRequired();
            entity.Property(a => a.MaxMarks).HasColumnType("decimal(6,2)");
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(a => a.ClassSubject)
                .WithMany(cs => cs.Assignments)
                .HasForeignKey(a => a.ClassSubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Teacher)
                .WithMany()
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasCheckConstraint("CK_Assignment_MaxMarks_Positive", "\"MaxMarks\" > 0");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
            entity.Property(s => s.AnswerText).IsRequired();
            entity.Property(s => s.Marks).HasColumnType("decimal(6,2)");
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.GradedByTeacher)
                .WithMany()
                .HasForeignKey(s => s.GradedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasCheckConstraint("CK_Submission_Marks_NonNegative", "\"Marks\" IS NULL OR \"Marks\" >= 0");
        });
    }
}
