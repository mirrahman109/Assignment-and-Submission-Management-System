using AssignmentSystem.Api.DTOs.Classes;
using AssignmentSystem.Api.DTOs.Subjects;
using AssignmentSystem.Api.DTOs.ClassSubjects;
using AssignmentSystem.Api.DTOs.TeacherAssignments;
using FluentValidation;

namespace AssignmentSystem.Api.Validators;

public class CreateClassCourseRequestValidator : AbstractValidator<CreateClassCourseRequest>
{
    public CreateClassCourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateClassCourseRequestValidator : AbstractValidator<UpdateClassCourseRequest>
{
    public UpdateClassCourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
    }
}

public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
    }
}

public class CreateClassSubjectRequestValidator : AbstractValidator<CreateClassSubjectRequest>
{
    public CreateClassSubjectRequestValidator()
    {
        RuleFor(x => x.ClassCourseId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
    }
}

public class CreateTeacherAssignmentRequestValidator : AbstractValidator<CreateTeacherAssignmentRequest>
{
    public CreateTeacherAssignmentRequestValidator()
    {
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.ClassSubjectId).GreaterThan(0);
    }
}
