using FluentValidation;
using InternHub.Application.DTO.Candidate;

namespace InternHub.Application.Validators.Candidate
{
    public class UpdateCandidateValidator : AbstractValidator<UpdateCandidateDto>
    {
        public UpdateCandidateValidator()
        {
            RuleFor(x => x.GitHubUrl)
                .Must(link => string.IsNullOrEmpty(link) || link.StartsWith("https://github.com/"))
                .WithMessage("Please write GitHub URL");

            RuleFor(x => x.Telegram)
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
