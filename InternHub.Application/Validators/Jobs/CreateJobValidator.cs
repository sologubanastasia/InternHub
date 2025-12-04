using FluentValidation;
using InternHub.Application.DTO.Company;

namespace InternHub.Application.Validators.Job
{
    public class CreateJobValidator : AbstractValidator<CreateJobDto>
    {
        public CreateJobValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Requirements)
                .NotEmpty()
                .MaximumLength(1000);

            RuleFor(x => x.Location)
                .MaximumLength(200);
        }
    }
}
