namespace InternHub.Application.Validators.Admin;

using InternHub.Application.DTOAdmin;
using FluentValidation;

public class ApproveCompanyValidator : AbstractValidator<ApproveCompanyDto>
{
    public void ApproveCompanyValidator()
    {
        RuleFor(x => x.Approve)
            .NotNull()
            .WithMessage("Approval decision must be choosed");
    }
}