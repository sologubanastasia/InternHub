namespace InternHub.Application.Validators.Auth;
using InternHub.Application.DTO.Auth;
using FluentValidation;

public class RegisterCompanyValidator : AbstractValidator<RegisterCompanyDto>
{
    public RegisterCompanyValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
           // .MinimumLength(150);

        RuleFor(x => x.CompanyName)
            .NotEmpty()
      //      .MaximumLength(150);

        RuleFor(x => x.Name)
            .NotEmpty()
       //     .MaximumLength(100);            
    }
}