using FluentValidation;
using InternHub.Application.DTO.Job;

namespace InternHub.Application.Validators.Job
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
