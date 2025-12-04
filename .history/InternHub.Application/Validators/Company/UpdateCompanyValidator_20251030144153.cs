using FluentValidation;
using InternHub.Application.DTO.Company;

namespace InternHub.Application.Validators.Сompany
{
    public class UpdateCompanyValidator : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.Website)
                .MaximumLength(250)
                .When(x => !string.IsNullOrEmpty(x.Website));
        }
    }
}
