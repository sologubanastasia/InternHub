using FluentValidation;
using InternHub.Application.DTO;

namespace InternHub.Application.Validators.Jobs
{
    public class ApplyJobValidator : AbstractValidator<JobApplicationCreateDto>
    {
        public ApplyJobValidator()
        {
            RuleFor(x => x.Message)
                .MaximumLength(500);
        }
    }
}
