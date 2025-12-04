using FluentValidation;
using InternHub.Application.DTO.Project;

namespace InternHub.Application.Validators.Project
{
    public class CreateProjectDtoValidation : AbstractValidator<CreateProjectDto>
    {
        public CreateProjectDtoValidation() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Назва проєкту обов'язкова.")
                .MaximumLength(200).WithMessage("Назва не може перевищувати 200 символів.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Опис проєкту обов'язковий.")
                .MaximumLength(300).WithMessage("Опис не може перевищувати 2000 символів.");
            
            RuleFor(x => x.RepositoryLink)
                .MaximumLength(250)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.RepositoryLink))
                .WithMessage("Некоректний формат URL репозиторію.");

            RuleFor(x => x.DemoVideoUrl)
                .MaximumLength(250)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.DemoVideoUrl))
                .WithMessage("Некоректний формат URL демо-відео.");
            
            RuleFor(x => x.IsTeamSearchActive)
                .NotNull().WithMessage("Статус пошуку команди має бути вказаний.");    

            RuleFor(x => x.ProjectTechnologies)
                .NotEmpty().WithMessage("Потрібно вказати хоча б одну технологію.");               
        }
    }
}