using FluentValidation;
using InternHub.Application.DTO.Team;
namespace InternHub.Application.Validators.Team
{
    public class ApplyToProjectDtoValidation : AbstractValidator<ApplyToProjectDto>
    {
        public ApplyToProjectDtoValidation()
        {
            RuleFor(x => x.Message)
                .MaximumLength(500);
        }
    }
}