using AssignmentSystem.Api.DTOs.Assignments;
using FluentValidation;

namespace AssignmentSystem.Api.Validators;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.ClassSubjectId).GreaterThan(0);
        RuleFor(x => x.MaxMarks).GreaterThan(0);
    }
}

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.MaxMarks).GreaterThan(0);
    }
}

public class UpdateAssignmentStatusRequestValidator : AbstractValidator<UpdateAssignmentStatusRequest>
{
    public UpdateAssignmentStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "Draft" or "Published")
            .WithMessage("Status must be Draft or Published.");
    }
}
