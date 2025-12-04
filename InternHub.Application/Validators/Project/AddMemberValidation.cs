using FluentValidation;
using InternHub.Application.DTO.Project;
namespace InternHub.Application.Validators.Project
{
    public class AddMemberValidation : AbstractValidator<AddMemberDto>
    {
        public AddMemberValidation()
        {
            RuleFor(x => x.CandidateId)
                .NotEmpty();

            RuleFor(x => x.Role)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}