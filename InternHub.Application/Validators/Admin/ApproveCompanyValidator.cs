namespace InternHub.Application.Validators.Admin;
using InternHub.Application.DTO.Admin;
using FluentValidation;

public class ApproveCompanyValidator : AbstractValidator<ApproveCompanyDto>
{
    public ApproveCompanyValidator()
    {
        RuleFor(x => x.Approve)
            .NotNull()
            .WithMessage("Approval decision must be choosed");
    }
}