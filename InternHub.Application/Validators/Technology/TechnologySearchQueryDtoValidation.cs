using FluentValidation;
using InternHub.Application.DTO.Pagination;
using InternHub.Application.DTO.Technology;

namespace InternHub.Application.Validators.Technology
{
    public class TechnologySearchQueryDtoValidation : AbstractValidator<TechnologySearchQueryDto>
    {
        public TechnologySearchQueryDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}    