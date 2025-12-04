using FluentValidation;
using InternHub.Application.DTO.Technology;

namespace InternHub.Application.Validators.Technology
{
    public class CreateTechnologyDtoValidation : AbstractValidator<CreateTechnologyDto>
    {
        public CreateTechnologyDtoValidation()
        {
            
        }
    }
}