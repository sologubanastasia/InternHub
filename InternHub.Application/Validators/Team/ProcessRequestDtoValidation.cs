using System.Data;
using FluentValidation;
using InternHub.Application.DTO.Project;
using InternHub.Application.DTO.Team;
namespace InternHub.Application.Validators.Team
{
    public class ProcessRequestDtoValidation : AbstractValidator<ProcessRequestDto>
    {
        public ProcessRequestDtoValidation()
        {
            RuleFor(x => x.Action)
                .IsInEnum();    

            RuleFor(x => x.Role)
                .MaximumLength(100);
        }
    }
}